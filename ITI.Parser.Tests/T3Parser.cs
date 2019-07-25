using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using NUnit.Framework;

namespace ITI.Parser.Tests
{
    [TestFixture]
    class T3Parser
    {
        private Parser parser = new Parser();

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

        public class EnumClass
        {
            public Color Colors;
            public Style Style;
        }

        [Test]
        public void t20_enum_member()
        {
            EnumClass value = parser.Parse<EnumClass>("{\"Colors\":\"Teal\",\"Style\":\"Blink, Dotted\"}");
            Assert.IsNotNull(value);
            Assert.AreEqual(Color.Teal, value.Colors);
            Assert.AreEqual(Style.Blink | Style.Dotted, value.Style);

            value = parser.Parse<EnumClass>("{\"Colors\":3,\"Style\":10}");
            Assert.IsNotNull(value);
            Assert.AreEqual(Color.Rainbow, value.Colors);
            Assert.AreEqual(Style.Dash | Style.Thin, value.Style);

            value = parser.Parse<EnumClass>("{\"Colors\":\"3\",\"Style\":\"10\"}");
            Assert.IsNotNull(value);
            Assert.AreEqual(Color.Rainbow, value.Colors);
            Assert.AreEqual(Style.Dash | Style.Thin, value.Style);

            value = parser.Parse<EnumClass>("{\"Colors\":\"sfdoijsdfoij\",\"Style\":\"sfdoijsdfoij\"}");
            Assert.IsNotNull(value);
            Assert.AreEqual(Color.Cyan, value.Colors);
            Assert.AreEqual(Style.None, value.Style);
        }
    }
}