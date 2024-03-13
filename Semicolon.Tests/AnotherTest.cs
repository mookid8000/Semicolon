using System;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using Semicolon.Attributes;
using Semicolon.Binding;
using Testy;
using Testy.Extensions;

namespace Semicolon.Tests;

[TestFixture]
public class AnotherTest : FixtureBase
{
    const string CsvText = @"

WhateverDate,JustNumber,ID,Money,Quantity,MoreMoney,""Quoted String""
04/11/2019,44,1,861.925,-11.5,-74.95,T
04/11/2019,45,1,861.925,-11.5,-74.95,T
04/11/2019,46,1,861.925,-11.5,-74.95,T
04/11/2019,47,1,-1311.25,-62.5,20.98,T
04/11/2019,47,2,-2643.75,-125,21.15,T
";

    [Test]
    public void CanParseIt()
    {
        var parser = new Parser<ExampleRow>(new Options { ColumnSeparator = ',' });

        var rows = parser.ParseCsv(CsvText.Trim()).ToList();

        rows.DumpTable();
    }

    class ExampleRow
    {
        [CsvColumn("WhateverDate", Binder = typeof(DayMonthYearBinder))]
        public DateTime Date { get; set; }

        [CsvColumn("JustNumber")]
        public int Number { get; set; }

        [CsvColumn("ID")]
        public int Id { get; set; }

        [CsvColumn("Money")]
        public decimal Money { get; set; }

        [CsvColumn("Quantity")]
        public decimal Quantity { get; set; }

        [CsvColumn("MoreMoney")]
        public decimal MoreMoney { get; set; }

        [CsvColumn("\"Quoted String\"")]
        public string QuotedString { get; set; }
    }

    class DayMonthYearBinder : IBinder
    {
        public object GetValue(CultureInfo culture, string str) => DateTime.ParseExact(str, "dd/MM/yyyy", DateTimeFormatInfo.InvariantInfo);
    }
}