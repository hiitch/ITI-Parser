using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace ITI.Parser
{
    public class Parser : IParser
    {
        private Stack<List<string>> _splitArrayPool = new Stack<List<string>>();
        private StringBuilder _stringBuilder;
        private Dictionary<Type, Dictionary<string, FieldInfo>> _fieldInfoCache = new Dictionary<Type, Dictionary<string, FieldInfo>>();
        private Dictionary<Type, Dictionary<string, PropertyInfo>> _propertyInfoCache = new Dictionary<Type, Dictionary<string, PropertyInfo>>();

        public T Parse<T>(string json)
        {
            if (_stringBuilder == null) _stringBuilder = new StringBuilder();

            json = RemoveWhitespace(json, true);

            return (T)ParseValue(typeof(T), json);
        }

        public string RemoveWhitespace(string json, bool appendEscapeCharacter)
        {
            if (_stringBuilder == null) _stringBuilder = new StringBuilder();
            _stringBuilder.Length = 0;

            for (int i = 0; i < json.Length; i++)
            {
                char c = json[i];

                if (c == SymbolCharacter.QuoteMark)
                {
                    i = AppendUntilStringEnd(appendEscapeCharacter, i, json);
                    continue;
                }
                if (char.IsWhiteSpace(c)) continue;

                _stringBuilder.Append(c);
            }
            return _stringBuilder.ToString();
        }

        public int AppendUntilStringEnd(bool appendEscapeCharacter, int startIdx, string json)
        {
            if (_stringBuilder == null) _stringBuilder = new StringBuilder();
            _stringBuilder.Append(json[startIdx]);

            for (int i = startIdx + 1; i < json.Length; i++)
            {
                if (json[i] == SymbolCharacter.EscapeChar)
                {
                    if (appendEscapeCharacter) _stringBuilder.Append(json[i]);

                    _stringBuilder.Append(json[i + 1]);
                    i++;
                }
                else if (json[i] == SymbolCharacter.QuoteMark)
                {
                    _stringBuilder.Append(json[i]);
                    return i;
                }
                else
                    _stringBuilder.Append(json[i]);
            }
            return json.Length - 1;
        }

        public object ParseValue(Type type, string json)
        {
            if (type == typeof(string)) return ParseTypeString(json);
            if (type.IsPrimitive)
            {
                var result = Convert.ChangeType(json, type, System.Globalization.CultureInfo.InvariantCulture);
                return result;
            }
            if (type == typeof(decimal))
                return ParseTypeDecimal(json);
            if (json == "null")
                return null;
            if (type.IsEnum)
                return ParseTypeEnum(json, type);
            if (type.IsArray)
                return ParseTypeArray(json, type);
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                return ParseTypeList(json, type);
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                return ParseTypeDictionary(json, type);
            if (json[0] == SymbolCharacter.OpenCur && json[json.Length - 1] == SymbolCharacter.CloseCur)
                return ParseObject(type, json);
            return null;
        }

        public string ParseTypeString(string json)
        {
            if (json.Length <= 2)
                return string.Empty;
            if (json[0] != SymbolCharacter.QuoteMark || json[json.Length - 1] != SymbolCharacter.QuoteMark)
                return null;

            StringBuilder parseStringBuilder = new StringBuilder(json.Length);
            for (int i = 1; i < json.Length - 1; ++i)
            {
                if ((json[i] == SymbolCharacter.EscapeChar) && IsEndOfJsonLength(json, i))
                {
                    int j = "\"\\nrtbf/".IndexOf(json[i + 1]);
                    if (j >= 0)
                    {
                        parseStringBuilder.Append("\"\\\n\r\t\b\f/"[j]);
                        ++i;
                        continue;
                    }
                    if (IsHexStructure(json, i))
                    {
                        UInt32 c = 0;
                        if (UInt32.TryParse(json.Substring(i + 2, 4), System.Globalization.NumberStyles.AllowHexSpecifier, null, out c))
                        {
                            parseStringBuilder.Append((char)c);
                            i += 5;
                            continue;
                        }
                    }
                }
                parseStringBuilder.Append(json[i]);
            }
            return parseStringBuilder.ToString();
        }

        public bool IsHexStructure(string json, int idx)
        {
            if ((json[idx + 1] == 'u') && idx + 5 < json.Length - 1) return true;
            return false;
        }

        private bool IsEndOfJsonLength(string json, int idx)
        {
            bool isLast = idx + 1 < json.Length - 1 ? true : false;
            return isLast;
        }

        public decimal ParseTypeDecimal(string json)
        {
            decimal result;
            decimal.TryParse(json, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out result);
            return result;
        }

        public object ParseTypeEnum(string json, Type type)
        {
            json = GetRidOfQuotesMarks(json);
            try
            {
                return Enum.Parse(type, json, false);
            }
            catch
            {
                return 0;
            }
        }

        public string GetRidOfQuotesMarks(string json)
        {
            if (string.IsNullOrEmpty(json))
                return null;
            if (json[0] == SymbolCharacter.QuoteMark && json[json.Length - 1] == SymbolCharacter.QuoteMark)
                return json.Substring(1, json.Length - 2);

            return json;
        }

        public object ParseTypeArray(string json, Type type)
        {
            Type arrayType = type.GetElementType();
            if (json[0] != SymbolCharacter.OpenSqr || json[json.Length - 1] != SymbolCharacter.CloseSqr)
                return null;

            List<string> elems = SplitCollection(json);
            Array newArray = Array.CreateInstance(arrayType, elems.Count);
            for (int i = 0; i < elems.Count; i++)
                newArray.SetValue(ParseValue(arrayType, elems[i]), i);
            _splitArrayPool.Push(elems);
            return newArray;
        }

        public object ParseTypeList(string json, Type type)
        {
            Type listType = type.GetGenericArguments()[0];
            if (json[0] != SymbolCharacter.OpenSqr || json[json.Length - 1] != SymbolCharacter.CloseSqr)
                return null;

            List<string> elems = SplitCollection(json);
            var list = (IList)type.GetConstructor(new Type[] { typeof(int) }).Invoke(new object[] { elems.Count });
            for (int i = 0; i < elems.Count; i++)
                list.Add(ParseValue(listType, elems[i]));
            _splitArrayPool.Push(elems);
            return list;
        }

        public object ParseTypeDictionary(string json, Type type)
        {
            Type keyType, valueType;
            {
                Type[] args = type.GetGenericArguments();
                keyType = args[0];
                valueType = args[1];
            }

            if (!CheckItIsDictionary(keyType, json))
                return null;

            List<string> elems = SplitCollection(json);

            var dictionary = (IDictionary)type.GetConstructor(new Type[] { typeof(int) }).Invoke(new object[] { elems.Count / 2 });
            for (int i = 0; i < elems.Count; i += 2)
            {
                if (elems[i].Length <= 2)
                    continue;
                string keyValue = elems[i].Substring(1, elems[i].Length - 2);
                object val = ParseValue(valueType, elems[i + 1]);
                dictionary.Add(keyValue, val);
            }
            return dictionary;
        }

        public bool CheckItIsDictionary(Type keyType, string json)
        {
            if (keyType != typeof(string))
                return false;
            if (json[0] != SymbolCharacter.OpenCur || json[json.Length - 1] != SymbolCharacter.CloseCur)
                return false;

            return true;
        }

        private object ParseObject(Type type, string json)
        {
            object instance = FormatterServices.GetUninitializedObject(type);

            //couple key/value, donc divisible par deux
            List<string> elems = SplitCollection(json);
            if (elems.Count % 2 != 0)
                return instance;

            Dictionary<string, FieldInfo> nameToField;
            Dictionary<string, PropertyInfo> nameToProperty;
            if (!_fieldInfoCache.TryGetValue(type, out nameToField))
            {
                nameToField = CreateMemberNameDictionary(type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy));
                _fieldInfoCache.Add(type, nameToField);
            }
            if (!_propertyInfoCache.TryGetValue(type, out nameToProperty))
            {
                nameToProperty = CreateMemberNameDictionary(type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy));
                _propertyInfoCache.Add(type, nameToProperty);
            }

            for (int i = 0; i < elems.Count; i += 2)
            {
                if (elems[i].Length <= 2)
                    continue;
                string key = elems[i].Substring(1, elems[i].Length - 2);
                string value = elems[i + 1];

                FieldInfo fieldInfo;
                PropertyInfo propertyInfo;
                if (nameToField.TryGetValue(key, out fieldInfo))
                    fieldInfo.SetValue(instance, ParseValue(fieldInfo.FieldType, value));
                else if (nameToProperty.TryGetValue(key, out propertyInfo))
                    propertyInfo.SetValue(instance, ParseValue(propertyInfo.PropertyType, value), null);
            }

            return instance;
        }

        public List<string> SplitCollection(string json)
        {
            List<string> splitArray = _splitArrayPool.Count > 0 ? _splitArrayPool.Pop() : new List<string>();
            splitArray.Clear();

            if (json.Length == 2)
                return splitArray;

            int parseDepth = 0;
            _stringBuilder.Length = 0;

            for (int i = 1; i < json.Length - 1; i++)
            {
                switch (json[i])
                {
                    case SymbolCharacter.OpenSqr:
                    case SymbolCharacter.OpenCur:
                        parseDepth++;
                        break;
                    case SymbolCharacter.CloseSqr:
                    case SymbolCharacter.CloseCur:
                        parseDepth--;
                        break;
                    case SymbolCharacter.QuoteMark:
                        i = AppendUntilStringEnd(true, i, json);
                        continue;
                    case SymbolCharacter.Comma:
                    case SymbolCharacter.DoublePoints:
                        if (parseDepth == 0)
                        {
                            splitArray.Add(_stringBuilder.ToString());
                            _stringBuilder.Length = 0;
                            continue;
                        }
                        break;
                }
                _stringBuilder.Append(json[i]);
            }
            splitArray.Add(_stringBuilder.ToString());

            return splitArray;
        }

        private Dictionary<string, T> CreateMemberNameDictionary<T>(T[] members) where T : MemberInfo
        {
            Dictionary<string, T> nameToMember = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < members.Length; i++)
            {
                T member = members[i];
                if (member.IsDefined(typeof(IgnoreDataMemberAttribute), true))
                    continue;

                string name = member.Name;
                if (member.IsDefined(typeof(DataMemberAttribute), true))
                {
                    DataMemberAttribute dataMemberAttribute = (DataMemberAttribute)Attribute.GetCustomAttribute(member, typeof(DataMemberAttribute), true);
                    if (!string.IsNullOrEmpty(dataMemberAttribute.Name))
                        name = dataMemberAttribute.Name;
                }
                nameToMember.Add(name, member);
            }
            return nameToMember;
        }
    }
}