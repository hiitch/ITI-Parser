using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using NUnit.Framework;

namespace ITI.Parser.Tests
{
    [TestFixture]
    public class PublicModelChecker
    {
        [Test]
        [Explicit]
        public void Write_current_public_API_to_console_with_double_quotes()
        {
            Console.WriteLine(GetPublicAPI(typeof( Parser ).Assembly).ToString().Replace("\"", "\"\""));
        }

        [Test]
        public void Public_API_is_not_modified()
        {
            var model = XElement.Parse(@"<Assembly Name=""ITI.Parser.Impl"">
                                            <Types>
                                              <Type Name=""ITI.Parser.Parser"">
                                                <Member Type=""Constructor"" Name="".ctor"" />
                                                <Member Type=""Method"" Name=""AppendUntilStringEnd"" />
                                                <Member Type=""Method"" Name=""CheckItIsDictionary"" />   
                                                <Member Type=""Method"" Name=""Equals"" />
                                                <Member Type=""Method"" Name=""GetHashCode"" />
                                                <Member Type=""Method"" Name=""GetRidOfQuotesMarks"" />
                                                <Member Type=""Method"" Name=""GetType"" />
                                                <Member Type=""Method"" Name=""IsHexStructure"" />
                                                <Member Type=""Method"" Name=""Parse"" />
                                                <Member Type=""Method"" Name=""ParseTypeArray"" />
                                                <Member Type=""Method"" Name=""ParseTypeDecimal"" />
                                                <Member Type=""Method"" Name=""ParseTypeDictionary"" />
                                                <Member Type=""Method"" Name=""ParseTypeEnum"" />
                                                <Member Type=""Method"" Name=""ParseTypeList"" />
                                                <Member Type=""Method"" Name=""ParseTypeString"" />
                                                <Member Type=""Method"" Name=""ParseValue"" />
                                                <Member Type=""Method"" Name=""RemoveWhitespace"" />
                                                <Member Type=""Method"" Name=""SplitCollection"" />
                                                <Member Type=""Method"" Name=""ToString"" />
                                              </Type>
                                            </Types>
                                          </Assembly>");

            var current = GetPublicAPI(typeof( Parser ).Assembly);
            if (!XElement.DeepEquals(model, current))
            {
                string m = model.ToString(SaveOptions.DisableFormatting);
                string c = current.ToString(SaveOptions.DisableFormatting);
                Assert.That(c, Is.EqualTo(m));
            }
        }

        XElement GetPublicAPI(Assembly a)
        {
            return new XElement("Assembly",
                                  new XAttribute("Name", a.GetName().Name),
                                  new XElement("Types",
                                                AllNestedTypes(a.GetExportedTypes())
                                                 .OrderBy(t => t.FullName)
                                                 .Select(t => new XElement("Type",
                                                                               new XAttribute("Name", t.FullName),
                                                                               t.GetMembers()
                                                                                .OrderBy(m => m.Name)
                                                                                .Select(m => new XElement("Member",
                                                                                                            new XAttribute("Type", m.MemberType),
                                                                                                            new XAttribute("Name", m.Name)))))));
        }

        IEnumerable<Type> AllNestedTypes(IEnumerable<Type> types)
        {
            foreach (Type t in types)
            {
                yield return t;
                foreach (Type nestedType in AllNestedTypes(t.GetNestedTypes()))
                {
                    yield return nestedType;
                }
            }
        }
    }
}
