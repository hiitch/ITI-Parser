using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using NUnit.Framework;

namespace ITI.Parser.Tests
{
    [TestFixture]
    public class T1Parser
    {
        private Parser parser = new Parser();
        
        void Test<T>(T expected, string json)
        {
            T value = parser.Parse<T>(json);
            Assert.AreEqual(expected, value);
        }

        [Test]
        public void t1_simple_values()
        {
            Test("hello", "\"hello\"");
            Test("hello there", "\"hello there\"");
            Test("hello\nthere", "\"hello\nthere\"");
            Test("hello\"there", "\"hello\\\"there\"");
            Test(String.Empty, "\"");
            Test(String.Empty, String.Empty);
            Test<object>(null, "aaa");
        }

        [Test]
        public void t2_let_unicode_hex16()
        {
            Test("\u94b1", "\"\u94b1\"");
            Test("\u94b1\u4e0d\u591f!", "\"\u94b1\u4e0d\u591f!\"");
            Test("\u94b1\u4e0d\u591f!", "\"\\u94b1\\u4e0d\\u591f!\"");
            Test("spi\u304C\u6700\u3082\u7F8E\u3057\u3044", "\"spi\\u304C\\u6700\\u3082\\u7F8E\\u3057\\u3044\"");
        }

        [Test]
        public void t3_numeric_values()
        {
            Test(12345L, "12345");
            Test(12345UL, "12345");
            Test(12.532f, "12.532");
            Test(12.532m, "12.532");
            Test(12.532d, "12.532");
        }

        [Test]
        public void t4_boolean_values()
        {
            Test(true, "true");
            Test(false, "false");
        }

        public enum Color
        {
            Cyan,
            Teal,
            Purple,
            Rainbow
        }

        [Flags]
        public enum Style
        {
            None = 0,
            Blink = 1,
            Dash = 2,
            Dotted = 4,
            Thin = 8
        }

        [Test]
        public void t5_enum_values()
        {
            Test(Color.Teal, "\"Teal\"");
            Test(Color.Purple, "2");
            Test(Color.Purple, "\"2\"");
            Test(Color.Cyan, "\"sfdoijsdfoij\"");
            Test(Style.Blink | Style.Dash, "\"Blink, Dash\"");
            Test(Style.Blink | Style.Dash, "3");
        }

        void ArrayTest<T>(T[] expected, string json)
        {
            var value = parser.Parse<T[]>(json);
            CollectionAssert.AreEqual(expected, value);
        }

        [Test]
        public void t6_array_values()
        {
            ArrayTest<object>(null, "[corrupted");
            ArrayTest<object>(null, "corrupted]");

            ArrayTest(new string[] { "one", "two", "three" }, "[\"one\",\"two\",\"three\"]");
            ArrayTest(new int[] { 1, 2, 3 }, "[1,2,3]");
            ArrayTest(new bool[] { true, false, true }, "     [true    ,    false,true     ]   ");
            ArrayTest(new object[] { null, null }, "[null,null]");
            ArrayTest(new float[] { 0.24f, 1.2f }, "[0.24,1.2]");
            ArrayTest(new double[] { 0.15, 0.19 }, "[0.15, 0.19]");
        }

        void ListTest<T>(List<T> expected, string json)
        {
            var value = parser.Parse<List<T>>(json);
            CollectionAssert.AreEqual(expected, value);
        }

        [Test]
        public void t7_list_values()
        {
            ListTest<object>(null, "[corrupted");
            ListTest<object>(null, "corrupted]");

            ListTest(new List<string> { "one", "two", "three" }, "[\"one\",\"two\",\"three\"]");
            ListTest(new List<int> { 1, 2, 3 }, "[1,2,3]");
            ListTest(new List<bool> { true, false, true }, "     [true    ,    false,true     ]   ");
            ListTest(new List<object> { null, null }, "[null,null]");
            ListTest(new List<float> { 0.24f, 1.2f }, "[0.24,1.2]");
            ListTest(new List<double> { 0.15, 0.19 }, "[0.15, 0.19]");
        }

        [Test]
        public void t8_recursive_arrays()
        {
            var expected = new int[][] { new int[] { 1, 2 }, new int[] { 3, 4 } };
            var actual = parser.Parse<int[][]>("[[1,2],[3,4]]");
            Assert.AreEqual(expected.Length, actual.Length);
            for (int i = 0; i < expected.Length; i++)
                CollectionAssert.AreEqual(expected[i], actual[i]);
        }

        [Test]
        public void t9_recursive_lists()
        {
            var expected = new List<List<int>> { new List<int> { 1, 2 }, new List<int> { 3, 4 } };
            var actual = parser.Parse<List<List<int>>>("[[1,2],[3,4]]");
            Assert.AreEqual(expected.Count, actual.Count);
            for (int i = 0; i < expected.Count; i++)
                CollectionAssert.AreEqual(expected[i], actual[i]);
        }
    }
}
