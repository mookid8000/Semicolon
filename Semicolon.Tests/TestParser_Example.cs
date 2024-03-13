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
public class TestParser_Example : FixtureBase
{
    [Test]
    [Description("This particular CSV example is characterized by having no (real) CSV headers")]
    public void ChewThroughBmReportsStuff()
    {
        var csv = string.Join(Environment.NewLine,
            Csv.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                .SkipWhile(line => line.StartsWith("HDR"))
                .TakeWhile(line => !line.StartsWith("FTR")));

        var options = new Options
        {
            ColumnSeparator = ',',
            Headers =
            {
                "Type",
                "Date",
                "TimeSlot"
            }
        };
        var rows = new Parser<BmRow>(options).ParseCsv(csv).ToList();

        rows.DumpTable();

        Assert.That(rows.Count, Is.EqualTo(18), "Expected 18 rows");
        Assert.That(rows.All(r => r.Date == new DateTime(2020, 12, 18)), Is.True);
        Assert.That(rows.Select(r => r.TimeSlot), Is.EqualTo(Enumerable.Range(1, 18)));
    }

    class PoorDateTimeBinder : IBinder
    {
        public object GetValue(CultureInfo culture, string str) => DateTime.ParseExact(str, "yyyyMMdd", CultureInfo.InvariantCulture);
    }

    class BmRow
    {
        [CsvColumn("Type")]
        public string LineType { get; set; }

        [CsvColumn("Date", Binder = typeof(PoorDateTimeBinder))]
        public DateTime Date { get; set; }

        [CsvColumn("TimeSlot")]
        public int TimeSlot { get; set; }
    }

    const string Csv = @"HDR,SYSTEM BUY SELL DATA
SSB,20201218,1,14.20000,14.20000,F,N,NULL,-452.6853,0.00,0.00,14.20,0.000,647.560,-936.248,647.560,-935.615,-544.500,380.500,-544.132,380.500
SSB,20201218,2,21.55000,21.55000,F,N,NULL,-69.2438,0.00,0.00,21.55,0.000,924.536,-829.801,924.536,-829.198,-544.500,380.500,-544.104,380.500
SSB,20201218,3,24.17000,24.17000,F,N,NULL,-47.9730,0.00,0.00,24.17,0.000,814.427,-698.402,814.427,-697.840,-544.500,380.500,-544.062,380.500
SSB,20201218,4,21.47000,21.47000,F,N,NULL,-97.0246,0.00,0.00,21.47,0.000,602.259,-535.300,602.259,-534.804,-544.500,380.500,-543.996,380.500
SSB,20201218,5,24.97000,24.97000,F,N,NULL,-51.9498,0.00,0.00,24.97,0.000,486.975,-457.449,486.975,-456.934,-462.000,380.500,-461.515,380.500
SSB,20201218,6,7.50000,7.50000,F,N,NULL,-161.0206,0.00,0.00,,,355.543,-435.063,355.543,-435.063,-462.000,380.500,-461.000,380.500
SSB,20201218,7,3.50000,3.50000,F,N,NULL,-379.1666,0.00,0.00,,,272.500,-537.667,272.500,-537.667,-494.500,380.500,-493.500,380.500
SSB,20201218,8,0.00000,0.00000,F,N,NULL,-666.6552,0.00,0.00,,,272.500,-825.155,272.500,-824.155,-494.500,380.500,-494.500,380.500
SSB,20201218,9,0.00000,0.00000,F,N,NULL,-609.1436,0.00,0.00,,,279.167,-796.810,279.167,-795.810,-472.000,380.500,-472.000,380.500
SSB,20201218,10,0.00000,0.00000,F,N,NULL,-481.0296,0.00,0.00,,,282.260,-671.789,282.260,-670.789,-472.000,380.500,-472.000,380.500
SSB,20201218,11,29.78000,29.78000,F,N,NULL,-115.5628,0.00,0.00,,,528.439,-480.002,528.439,-480.002,-544.500,380.500,-543.500,380.500
SSB,20201218,12,64.00000,64.00000,F,P,NULL,97.1734,0.00,0.00,,,578.515,-329.867,577.515,-329.867,-544.500,393.025,-544.500,393.025
SSB,20201218,13,65.00000,65.00000,F,P,0.00000,102.8648,0.00,0.00,,,428.806,-324.000,427.806,-324.000,-384.500,382.559,-384.500,382.559
SSB,20201218,14,65.00000,65.00000,F,P,0.00000,182.2877,0.00,0.00,,,513.279,-227.042,512.279,-227.042,-384.500,280.500,-384.500,280.500
SSB,20201218,15,55.00000,55.00000,F,P,0.00000,55.1739,0.00,0.00,,,388.943,-641.688,388.943,-641.688,-192.000,500.000,-192.000,499.000
SSB,20201218,16,56.24000,56.24000,F,P,0.00000,116.6665,0.00,0.00,,,663.242,-854.575,663.242,-854.575,-192.000,500.000,-192.000,499.000
SSB,20201218,17,23.75000,23.75000,F,N,0.00000,-152.0601,0.00,0.00,,,428.775,-638.835,428.775,-637.835,-192.000,250.000,-192.000,250.000
SSB,20201218,18,56.98000,56.98000,F,P,0.00000,95.9678,0.00,0.00,,,418.250,-380.282,418.250,-380.282,-192.000,250.000,-192.000,249.000
FTR,18";
}