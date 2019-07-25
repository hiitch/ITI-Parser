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
            throw new NotImplementedException();
        }

        public string RemoveWhitespace(string json, bool appendEscapeCharacter)
        {
            throw new NotImplementedException();
        }

        public int AppendUntilStringEnd(bool appendEscapeCharacter, int startIdx, string json)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public bool IsHexStructure(string json, int idx)
        {
            throw new NotImplementedException();
        }

        private bool IsEndOfJsonLength(string json, int idx)
        {
            bool isLast = idx + 1 < json.Length - 1 ? true : false;
            return isLast;
        }

        public decimal ParseTypeDecimal(string json)
        {
            throw new NotImplementedException();
        }

        public object ParseTypeEnum(string json, Type type)
        {
            throw new NotImplementedException();
        }

        public string GetRidOfQuotesMarks(string json)
        {
            throw new NotImplementedException();
        }

        public object ParseTypeArray(string json, Type type)
        {
            throw new NotImplementedException();
        }

        public object ParseTypeList(string json, Type type)
        {
            throw new NotImplementedException();
        }

        public object ParseTypeDictionary(string json, Type type)
        {
            throw new NotImplementedException();
        }

        public bool CheckItIsDictionary(Type keyType, string json)
        {
            throw new NotImplementedException();
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