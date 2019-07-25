using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using NUnit.Framework;

namespace ITI.Parser.Tests
{
    [TestFixture]
    class T2Parser
    {
        private Parser parser = new Parser();

        public void DictTest<K, V>(Dictionary<K, V> expected, string json)
        {
            var value = parser.Parse<Dictionary<K, V>>(json);
            Assert.AreEqual(expected.Count, value.Count);
            foreach (var pair in expected)
            {
                Assert.IsTrue(value.ContainsKey(pair.Key));
                Assert.AreEqual(pair.Value, value[pair.Key]);
            }
        }

        [Test]
        public void t10_dictionary_values()
        {
            Assert.IsNull(parser.Parse<Dictionary<int, int>>("{0:5}"));
            Assert.IsNull(parser.Parse<Dictionary<int, int>>("\"1\":5,\"2\":10,\"3\":15}"));
            Assert.IsNull(parser.Parse<Dictionary<int, int>>("{\"foo\":5,\"bar\":10,\"baz\":128"));

            DictTest(new Dictionary<string, int> { { "foo", 5 }, { "bar", 10 }, { "baz", 128 } }, "{\"foo\":5,\"bar\":10,\"baz\":128}");
            DictTest(new Dictionary<string, float> { { "foo", 5f }, { "bar", 10f }, { "baz", 128f } }, "{\"foo\":5,\"bar\":10,\"baz\":128}");
            DictTest(new Dictionary<string, string> { { "foo", "\"" }, { "bar", "hello" }, { "baz", "," } }, "{\"foo\":\"\\\"\",\"bar\":\"hello\",\"baz\":\",\"}");
        }

        [Test]
        public void t11_recursive_dictionary()
        {
            var result = parser.Parse<Dictionary<string, Dictionary<string, string>>>("{\"foo\":{ \"bar\":\"\\\"{,,:[]}\" }}");
            Assert.AreEqual("\"{,,:[]}", result["foo"]["bar"]);
        }

        class SimpleObject
        {
            public int PV;
            public float MP;
            public string XP { get; set; }
            public List<int> Coord { get; set; }
        }

        [Test]
        public void t12_simple_object()
        {
            SimpleObject value = parser.Parse<SimpleObject>("{\"PV\":123,\"MP\":456,\"XP\":\"789\",\"Coord\":[10,11,12]}");
            Assert.IsNotNull(value);
            Assert.AreEqual(123, value.PV);
            Assert.AreEqual(456f, value.MP);
            Assert.AreEqual("789", value.XP);
            CollectionAssert.AreEqual(new List<int> { 10, 11, 12 }, value.Coord);

            value = parser.Parse<SimpleObject>("dfpoksdafoijsdfij");
            Assert.IsNull(value);
        }

        struct SimpleStruct
        {
            public SimpleObject Obj;
        }

        [Test]
        public void t13_simple_struct()
        {
            SimpleStruct value = parser.Parse<SimpleStruct>("{\"obj\":{\"PV\":12345}}");
            Assert.IsNotNull(value.Obj);
            Assert.AreEqual(value.Obj.PV, 12345);
        }

        struct SmallStruct
        {
            public int Value;
        }

        [Test]
        public void t14_list_of_struct()
        {
            var values = parser.Parse<List<SmallStruct>>("[{\"Value\":1},{\"Value\":2},{\"Value\":3}]");
            for (int i = 0; i < values.Count; i++)
                Assert.AreEqual(i + 1, values[i].Value);
        }

        class ComplexObject
        {
            public ComplexObject voiture;
            public List<ComplexObject> elems;
            public SimpleStruct structure;
        }

        [Test]
        public void t15_complex_object()
        {
            var value = parser.Parse<ComplexObject>("{\"voiture\":{\"voiture\":{\"voiture\":{}}}}");
            Assert.IsNotNull(value);
            Assert.IsNotNull(value.voiture);
            Assert.IsNotNull(value.voiture.voiture);
            Assert.IsNotNull(value.voiture.voiture.voiture);

            value = parser.Parse<ComplexObject>("{\"elems\":[{},null,{\"voiture\":{}}]}");
            Assert.IsNotNull(value);
            Assert.IsNotNull(value.elems);
            Assert.IsNotNull(value.elems[0]);
            Assert.IsNull(value.elems[1]);
            Assert.IsNotNull(value.elems[2].voiture);

            value = parser.Parse<ComplexObject>("{\"structure\":{\"Obj\":{\"PV\":5}}}");
            Assert.IsNotNull(value);
            Assert.IsNotNull(value.structure.Obj);
            Assert.AreEqual(5, value.structure.Obj.PV);
        }

        public struct ComplexStruct
        {
            public byte R, V, B;
            public ComplexStruct(byte r, byte v, byte b)
            {
                R = r; V = v; B = b;
            }
            public static ComplexStruct Nasty = new ComplexStruct(0, 0, 0);
        }

        [Test]
        public void t16_complex_struct()
        {
            ComplexStruct s = parser.Parse<ComplexStruct>("{\"R\":234,\"V\":123,\"B\":11}");
            Assert.AreEqual(234, s.R);
            Assert.AreEqual(123, s.V);
            Assert.AreEqual(11, s.B);
        }

        [Test]
        public void t17_dictionary_and_escaping_value()
        {
            var orig = new Dictionary<string, string> { { "hello", "world\n \" \\ \b \r \\0\u263A" } };
            var parsed = parser.Parse<Dictionary<string, string>>("{\"hello\":\"world\\n \\\" \\\\ \\b \\r \\0\\u263a\"}");
            Assert.AreEqual(orig["hello"], parsed["hello"]);
        }

        class IgnoreDataMemberObject
        {
            public int A;
            [IgnoreDataMember]
            public int B;

            public int C { get; set; }
            [IgnoreDataMember]
            public int D { get; set; }
        }

        [Test]
        public void t18_ignore_data_member()
        {
            IgnoreDataMemberObject value = parser.Parse<IgnoreDataMemberObject>("{\"A\":123,\"B\":456,\"Ignored\":10,\"C\":789,\"D\":14}");
            Assert.IsNotNull(value);
            Assert.AreEqual(123, value.A);
            Assert.AreEqual(0, value.B);
            Assert.AreEqual(789, value.C);
            Assert.AreEqual(0, value.D);
        }

        class DataMemberObject
        {
            [DataMember(Name = "a")]
            public int A;
            [DataMember()]
            public int B;

            [DataMember(Name = "c")]
            public int C { get; set; }
            public int D { get; set; }
        }

        [Test]
        public void t19_data_member_object()
        {
            DataMemberObject value = parser.Parse<DataMemberObject>("{\"a\":123,\"B\":456,\"c\":789,\"D\":14}");
            Assert.IsNotNull(value);
            Assert.AreEqual(123, value.A);
            Assert.AreEqual(456, value.B);
            Assert.AreEqual(789, value.C);
            Assert.AreEqual(14, value.D);
        }
    }
}