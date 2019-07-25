using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using NUnit.Framework;

namespace ITI.Parser.Tests
{
    [TestFixture]
    public class T0Parser
    {
        private Parser parser = new Parser();

        [Test]
        public void impl01_remove_whitespace_and_append_until_string_end()
        {
            Assert.AreEqual("aaaa", parser.RemoveWhitespace("aa aa", true));
            Assert.AreEqual("aaaa", parser.RemoveWhitespace(" aa aa", true));
            Assert.AreEqual("bbbb", parser.RemoveWhitespace("b  b   b   b", true));

            Assert.AreEqual("\"hello\there\"", parser.RemoveWhitespace("\"hello\there\"", true));
            Assert.AreEqual("\"hellothere\"", parser.RemoveWhitespace("\"hello\\there\"", false));
        }

        [Test]
        public void impl03_is_hex_structure()
        {
            Assert.AreEqual(true, parser.IsHexStructure("\\u94b1!", 0));
            Assert.AreEqual(true, parser.IsHexStructure("\\u94b1\"", 0));
            Assert.AreEqual(false, parser.IsHexStructure("\\u94b1", 0));
        }

        [Test]
        public void impl04_get_rid_of_quotes_marks()
        {
            Assert.AreEqual(null, parser.GetRidOfQuotesMarks(""));
            Assert.AreEqual(null, parser.GetRidOfQuotesMarks(null));
            Assert.AreEqual("azerty\"", parser.GetRidOfQuotesMarks("azerty\""));
            Assert.AreEqual("\"azerty", parser.GetRidOfQuotesMarks("\"azerty"));
            Assert.AreEqual("poiuy", parser.GetRidOfQuotesMarks("\"poiuy\""));
        }

        [Test]
        public void impl05_check_it_is_dictionary()
        {
            Assert.AreEqual(false, parser.CheckItIsDictionary(typeof(List), "[\"1\",\"2\",\"3\"]"));
            Assert.AreEqual(false, parser.CheckItIsDictionary(typeof(Int32), "123"));
            Assert.AreEqual(true, parser.CheckItIsDictionary(typeof(string), "{\"123\"}"));
            Assert.AreEqual(false, parser.CheckItIsDictionary(typeof(string), "\"abc\"}"));
            Assert.AreEqual(false, parser.CheckItIsDictionary(typeof(string), "{\"abc\""));
        }

        [Test]
        public void impl06_parse_type_string()
        {
            Assert.AreEqual(string.Empty, parser.ParseTypeString("je"));
            Assert.AreEqual(string.Empty, parser.ParseTypeString("o"));
            Assert.IsNull(parser.ParseTypeString("\"azerty"));
            Assert.IsNull(parser.ParseTypeString("azerty\""));
            Assert.AreEqual("hello\nthere", parser.ParseTypeString("\"hello\\nthere\""));
            Assert.AreEqual("hello\rthere", parser.ParseTypeString("\"hello\\rthere\""));
            Assert.AreEqual("hello\tthere", parser.ParseTypeString("\"hello\\tthere\""));
            Assert.AreEqual("hello\bthere", parser.ParseTypeString("\"hello\\bthere\""));
            Assert.AreEqual("hello\fthere", parser.ParseTypeString("\"hello\\fthere\""));
            Assert.AreEqual("hello\\xthere", parser.ParseTypeString("\"hello\\xthere\""));
            Assert.AreEqual("hello\\hthere", parser.ParseTypeString("\"hello\\hthere\""));
            Assert.AreEqual("\u94b1", parser.ParseTypeString("\"\u94b1\""));
            Assert.AreEqual("\u94b1\u4e0d\u591f!", parser.ParseTypeString("\"\u94b1\u4e0d\u591f!\""));
        }

        [Test]
        public void impl07_parse_type_decimal()
        {
            Assert.AreEqual(777L, parser.ParseTypeDecimal("777"));
            Assert.AreEqual(77.7f, parser.ParseTypeDecimal("77.7"));
            Assert.AreEqual(77.7d, parser.ParseTypeDecimal("77.7"));
            Assert.AreEqual(77.7m, parser.ParseTypeDecimal("77.7"));
        }

        public enum Car
        {
            Bmw,
            Mercedes,
            Honda
        }

        [Test]
        public void impl08_parse_type_enum()
        {
            Assert.AreEqual(0, parser.ParseTypeEnum(String.Empty, typeof(string)));
            Assert.AreEqual(0, parser.ParseTypeEnum(null, typeof(string)));
            Assert.AreEqual(Car.Bmw, parser.ParseTypeEnum("\"Bmw\"", typeof(Car)));
            Assert.AreEqual(Car.Honda, parser.ParseTypeEnum("\"Honda\"", typeof(Car)));
        }

        [Test]
        public void impl09_parse_type_array()
        {
            Assert.IsNull(parser.ParseTypeArray("\"1\",\"2\",\"3\"]", typeof(string[])));
            Assert.IsNull(parser.ParseTypeArray("[\"1\",\"2\",\"3\"", typeof(string[])));
            Assert.IsNull(parser.ParseTypeArray("{\"1\",\"2\",\"3\"}", typeof(string[])));
            Assert.AreEqual(new string[] { "1", "2", "3" }, parser.ParseTypeArray("[\"1\",\"2\",\"3\"]", typeof(string[])));
            Assert.AreEqual(new int[] { 1, 2, 3 }, parser.ParseTypeArray("[1,2,3]", typeof(int[])));
        }

        [Test]
        public void impl10_parse_type_list()
        {
            Assert.IsNull(parser.ParseTypeList("\"1\",\"2\",\"3\"]", typeof(List<string>)));
            Assert.IsNull(parser.ParseTypeList("[\"1\",\"2\",\"3\"", typeof(List<string>)));
            Assert.IsNull(parser.ParseTypeList("{\"1\",\"2\",\"3\"}", typeof(List<string>)));
            Assert.AreEqual(new List<string> { "1", "2", "3" }, parser.ParseTypeList("[\"1\",\"2\",\"3\"]", typeof(List<string>)));
            Assert.AreEqual(new List<int> { 1, 2, 3 }, parser.ParseTypeList("[1,2,3]", typeof(List<int>)));
        }

        [Test]
        public void impl11_parse_type_dictionary()
        {
            Dictionary<string, string> newDictionary1 = new Dictionary<string, string>();
            newDictionary1.Add("Cyan", "Blue");
            Dictionary<string, string> newDictionary2 = new Dictionary<string, string>();
            newDictionary2.Add("Cyan", "Blue");
            newDictionary2.Add("Bruise", "Blue");
            Dictionary<string, int> newDictionary3 = new Dictionary<string, int>();
            newDictionary3.Add("seventy", 70);
            newDictionary3.Add("forty-two", 42);
            Assert.AreEqual(newDictionary1, parser.ParseTypeDictionary("{\"Cyan\":\"Blue\"}", typeof(Dictionary<string, string>)));
            Assert.AreEqual(newDictionary2, parser.ParseTypeDictionary("{\"Cyan\":\"Blue\",\"Bruise\":\"Blue\"}", typeof(Dictionary<string, string>)));
            Assert.AreEqual(newDictionary3, parser.ParseTypeDictionary("{\"seventy\":70,\"forty-two\":42}", typeof(Dictionary<string, int>)));
        }
    }
}
