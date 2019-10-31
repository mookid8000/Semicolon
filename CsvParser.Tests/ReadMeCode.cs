using System;
using System.Globalization;
using CsvParser.Attributes;
using CsvParser.Binding;
using NUnit.Framework;
using Testy.Extensions;

namespace CsvParser.Tests
{
    [TestFixture]
    public class ReadMeCode
    {
        const string CsvText = @"Player ID;Name;Duration (s)
734;Joe;65
78439;Moe;63
12342;Bo;67
";

        [Test]
        public void Example()
        {
            var parser = new Parser<CsvRow>();

            var rows = parser.ParseCsv(CsvText);

            rows.DumpTable();
        }

        class CsvRow
        {
            [CsvColumn("Player ID")]
            public string Id { get; set; }

            [CsvColumn("Name")]
            public string Name { get; set; }

            [CsvColumn("Duration (s)", Binder = typeof(ConvertSecondsToTimeSpanBinder))]
            public TimeSpan Duration { get; set; }
        }

        class ConvertSecondsToTimeSpanBinder : IBinder
        {
            public object GetValue(CultureInfo culture, string str) => TimeSpan.FromSeconds(int.Parse(str));
        }
    }
}