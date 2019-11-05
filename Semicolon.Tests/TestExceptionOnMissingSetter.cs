using System;
using System.Linq;
using NUnit.Framework;
using Semicolon.Attributes;
using Semicolon.Exceptions;
using Testy;
// ReSharper disable ReturnValueOfPureMethodIsNotUsed

namespace Semicolon.Tests
{
    [TestFixture]
    public class TestExceptionOnMissingSetter : FixtureBase
    {
        [Test]
        public void CheckException()
        {
            var parser = new Parser<RowType>();

            var exception = Assert.Throws<CsvParserException>(() =>
            {
                parser
                    .ParseCsv(@"Text
Hej
Med
Dig")
                    .ToList();
            });

            Console.WriteLine(exception);

            Assert.That(exception.ToString(),
                Contains.Substring("This is probably an indication that the property does not have a public setter"));
        }

        class RowType
        {
            [CsvColumn("Text")]
            public string NoSetter { get; private set; }
        }
    }
}