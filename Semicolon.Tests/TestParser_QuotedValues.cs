﻿using System;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using Semicolon.Attributes;
using Semicolon.Binding;
using Testy.Extensions;

namespace Semicolon.Tests;

[TestFixture]
public class TestParser_QuotedValues
{
    [Test]
    public void QuotedValuesWithSeparatorsInThem()
    {
        const string csv = @"""Full Export Completed"";""Partial Result Timestamp"";""Filters""
""YES"";"""";""Campaign: DK1; Campaign: DK2""
""NO"";"""";""Campaign: DK1; Campaign: DK2; Campaign: DK3""
";

        var parser = new Parser<CampaignModel>(new() { ColumnSeparator = ';', ValueDelimiter = '"' });

        var rows = parser.ParseCsv(csv).ToList();

        rows.DumpTable();

        Assert.That(rows.Count, Is.EqualTo(2));
        Assert.That(rows.Select(r => new { r.FullExportCompleted, r.Filters }), Is.EqualTo(new[]
        {
            new { FullExportCompleted = "YES", Filters = "Campaign: DK1; Campaign: DK2" },
            new { FullExportCompleted = "NO", Filters = "Campaign: DK1; Campaign: DK2; Campaign: DK3" },
        }));
    }

    class CampaignModel
    {
        [CsvColumn("Full Export Completed")]
        public string FullExportCompleted { get; set; }

        [CsvColumn("Filters")]
        public string Filters { get; set; }
    }

    [Test]
    public void TennetExample()
    {
        const string csv =
            @"""Date"",""PTE"",""period_from"",""period_until"",""upward_incident_reserve"",""downward_incident_reserve"",""To regulate up"",""To regulate down"",""Incentive component"",""Consume"",""Feed"",""Regulation state""
""01/17/2021"",""1"",""00:00"",""00:15"","""","""","""",""-60.77"",""0"",""-60.77"",""-60.77"",""-1""
""01/17/2021"",""2"",""00:15"",""00:30"","""","""","""","""",""0"",""44.78"",""44.78"",""0""
""01/17/2021"",""3"",""00:30"",""00:45"","""","""","""",""41.5"",""0"",""41.5"",""41.5"",""-1""
""01/17/2021"",""4"",""00:45"",""01:00"","""","""","""",""39.46"",""0"",""39.46"",""39.46"",""-1""
""01/17/2021"",""5"",""01:00"",""01:15"","""","""",""61.51"",""39.46"",""0"",""61.51"",""61.51"",""1""
""01/17/2021"",""6"",""01:15"",""01:30"","""","""",""52.35"","""",""0"",""52.35"",""52.35"",""1""
""01/17/2021"",""7"",""01:30"",""01:45"","""","""","""","""",""0"",""45.27"",""45.27"",""0""
""01/17/2021"",""8"",""01:45"",""02:00"","""","""","""",""26.14"",""0"",""26.14"",""26.14"",""-1""
""01/17/2021"",""9"",""02:00"",""02:15"","""","""",""48.78"",""38.52"",""0"",""48.78"",""38.52"",""2""
""01/17/2021"",""10"",""02:15"",""02:30"","""","""","""","""",""0"",""44.68"",""44.68"",""0""
""01/17/2021"",""11"",""02:30"",""02:45"","""","""","""","""",""0"",""44.78"",""44.78"",""0""
""01/17/2021"",""12"",""02:45"",""03:00"","""","""","""",""44.71"",""0"",""44.71"",""44.71"",""-1""
""01/17/2021"",""13"",""03:00"",""03:15"","""","""",""63.25"","""",""0"",""63.25"",""63.25"",""1""
""01/17/2021"",""14"",""03:15"",""03:30"","""","""",""48.79"","""",""0"",""48.79"",""48.79"",""1""
""01/17/2021"",""15"",""03:30"",""03:45"","""","""","""","""",""0"",""44.73"",""44.73"",""0""";

        var options = new Options { ColumnSeparator = ',', ValueDelimiter = '"' };
        var parser = new Parser<TennetRow>(options);

        var rows = parser.ParseCsv(csv).ToList();

        rows.DumpTable();
    }

    [Test]
    public void AnotherExample()
    {
        const string csv =
            @"""Full Export Completed"";""Partial Result Timestamp"";""Filters"";""Users"";""Date"";""Duration"";""Total ACW"";""Direction"";""Queue"";""Wrap-up"";""Last Wrap Up"";""Campaign Name"";""ANI"";""DNIS"";""outb Attempted""
""YES"";"""";""CMP1; CMP2; CMP3; CMP4; CMP5; CMP6"";"""";""6/4/24 10:01 AM"";"" 00:00:14.191"";"""";""outb"";""outyes"";""CODE-BUSY"";""CODE-BUSY"";""CNCPT1"";""tel:+4511223344"";""tel:+4588995566"";""1""
""YES"";"""";""CMP1; CMP2; CMP3; CMP4; CMP5; CMP6"";"""";""6/4/24 10:01 AM"";"" 00:00:26.739"";"""";""inb/outb"";""outyes"";""tlfsvarer; CODE-GOTO-FLOW"";""tlfsvarer"";""CNCPT1"";""tel:+4522554477"";""tel:+4566996699"";""1""
""YES"";"""";""CMP1; CMP2; CMP3; CMP4; CMP5; CMP6"";"""";""6/4/24 10:01 AM"";"" 00:00:20.204"";"""";""outb"";""outyes"";""CODE-NO-ANSWER"";""CODE-NO-ANSWER"";""CNCPT1"";""tel:+4522334455"";""tel:+4577887788"";""1""
""YES"";"""";""CMP1; CMP2; CMP3; CMP4; CMP5; CMP6"";"""";""6/4/24 10:01 AM"";"" 00:00:26.641"";"""";""inb/outb"";""outyes"";""tlfsvarer; CODE-GOTO-FLOW"";""tlfsvarer"";""CNCPT1"";""tel:+4500110011"";""tel:+4522335544"";""1""";

        var options = new Options { ColumnSeparator = ';', ValueDelimiter = '"' };
        var parser = new Parser<AnotherRow>(options);

        var rows = parser.ParseCsv(csv).ToList();

        rows.DumpTable();

        Assert.That(rows.Count, Is.EqualTo(4));
        Assert.That(rows.Select(r => r.Value), Is.EqualTo(new[]
        {
            "tel:+4588995566",
            "tel:+4566996699",
            "tel:+4577887788",
            "tel:+4522335544",
        }));
    }

    [Test]
    public void VerifyNiceExceptionWhenAccidentallySwitchingConfigurationValues()
    {
        const string csv =
            @"""Full Export Completed"";""Partial Result Timestamp"";""Filters"";""Users"";""Date"";""Duration"";""Total ACW"";""Direction"";""Queue"";""Wrap-up"";""Last Wrap Up"";""Campaign Name"";""ANI"";""DNIS"";""outb Attempted""
""YES"";"""";""CMP1; CMP2; CMP3; CMP4; CMP5; CMP6"";"""";""6/4/24 10:01 AM"";"" 00:00:14.191"";"""";""outb"";""outyes"";""CODE-BUSY"";""CODE-BUSY"";""CNCPT1"";""tel:+4511223344"";""tel:+4588995566"";""1""
""YES"";"""";""CMP1; CMP2; CMP3; CMP4; CMP5; CMP6"";"""";""6/4/24 10:01 AM"";"" 00:00:26.739"";"""";""inb/outb"";""outyes"";""tlfsvarer; CODE-GOTO-FLOW"";""tlfsvarer"";""CNCPT1"";""tel:+4522554477"";""tel:+4566996699"";""1""
""YES"";"""";""CMP1; CMP2; CMP3; CMP4; CMP5; CMP6"";"""";""6/4/24 10:01 AM"";"" 00:00:20.204"";"""";""outb"";""outyes"";""CODE-NO-ANSWER"";""CODE-NO-ANSWER"";""CNCPT1"";""tel:+4522334455"";""tel:+4577887788"";""1""
""YES"";"""";""CMP1; CMP2; CMP3; CMP4; CMP5; CMP6"";"""";""6/4/24 10:01 AM"";"" 00:00:26.641"";"""";""inb/outb"";""outyes"";""tlfsvarer; CODE-GOTO-FLOW"";""tlfsvarer"";""CNCPT1"";""tel:+4500110011"";""tel:+4522335544"";""1""";

        var options = new Options { ColumnSeparator = '"', ValueDelimiter = ';' };
        var parser = new Parser<AnotherRow>(options);

        var exception = Assert.Throws<FormatException>(() => _ = parser.ParseCsv(csv).ToList())!;

        Console.WriteLine(exception);

        Assert.That(exception.Message, Contains.Substring("The error occurred at position 1 in the line ('F')"));
        Assert.That(exception.Message, Contains.Substring(@"""Full Export Completed"";""Partial Result Timestamp"";""Filters"";""Users"";""Date"";""Duration"";""Total ACW"";""Direction"";""Queue"";""Wrap-up"";""Last Wrap Up"";""Campaign Name"";""ANI"";""DNIS"";""outb Attempted"""));
    }

    class AnotherRow
    {
        [CsvColumn("DNIS")]
        public string Value { get; set; }
    }

    class TennetRow
    {
        [CsvColumn("Date", Binder = typeof(Date.Binder))]
        public Date Date { get; set; }

        [CsvColumn("PTE")]
        public int Period { get; set; }

        [CsvColumn("period_from")]
        public string PeriodFrom { get; set; }

        [CsvColumn("period_until")]
        public string PeriodUntil { get; set; }
    }

    class Date
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }

        public override string ToString() => $"{Year:0000}-{Month:00}-{Day:00}";

        public class Binder : IBinder
        {
            public object GetValue(CultureInfo culture, string str)
            {
                var parts = str.Split('/');

                return new Date
                {
                    Day = int.Parse(parts[0]),
                    Month = int.Parse(parts[1]),
                    Year = int.Parse(parts[2]),
                };
            }
        }
    }
}