using System;
using System.Globalization;
using NUnit.Framework;
using Semicolon.Attributes;
using Semicolon.Binding;
using Testy.Extensions;

namespace Semicolon.Tests;

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