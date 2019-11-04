using System;
using System.Globalization;
using NUnit.Framework;
using Semicolon.Attributes;
using Semicolon.Binding;
using Testy;
using Testy.Extensions;

namespace Semicolon.Tests
{
    [TestFixture]
    public class TestParser : FixtureBase
    {
        [Test]
        public void CanParseCsv()
        {
            var parser = new Parser<CsvRow>();

            parser.ParseCsv(CsvText).DumpTable();
        }

        const string CsvText = @"Id;Fornavn(e);Efternavn;Email;Dato;float;double;decimal;byte;short;int;long;En anden dato;Endnu en anden dato;Multi;Time zone;Culture
c0ffee;Mogens Heller;Grabe;mogens@rebus.fm;2019-10-31;1.2;1.2;1.2;23;30000;1000000000;83748437849378493;2019-11-01;2019-11-01;Hej;Romance Standard Time;da-DK
bada55;Mogens Heller;Grabe;mookid8000@gmail.com;31-10-2019;1.2;1.2;1.2;23;30000;1000000000;83748437849378493;2019-11-01;2019-11-01;Hej;Romance Standard Time;da-DK";

        class CsvRow
        {
            [CsvColumn("Id")]
            public string Id { get; set; }

            [CsvColumn("Fornavn(e)")]
            public string FirstName { get; set; }

            [CsvColumn("Efternavn")]
            public string LastName { get; set; }

            [CsvColumn("Email")]
            public string Email { get; set; }

            [CsvColumn("Dato", Binder = typeof(CustomDateTimeBinder))]
            public DateTime Date { get; set; }

            [CsvColumn("float")]
            public float Float { get; set; }

            [CsvColumn("byte")]
            public byte Byte { get; set; }

            [CsvColumn("short")]
            public short Short { get; set; }

            [CsvColumn("int")]
            public int Int { get; set; }

            [CsvColumn("long")]
            public long Long { get; set; }
            
            [CsvColumn("En anden dato")]
            public DateTime DateTime { get; set; }

            [CsvColumn("Endnu en anden dato")]
            public DateTimeOffset DateTimeOffset { get; set; }

            [CsvColumn("Multi")]
            public string FirstProperty { get; set; }

            [CsvColumn("Multi")]
            public string SecondProperty { get; set; }

            [CsvColumn("Culture")]
            public CultureInfo CultureInfo { get; set; }

            [CsvColumn("Time zone")]
            public TimeZoneInfo TimeZoneInfo { get; set; }
        }

        /// <summary>
        /// Custom date binder that accepts both yyyy-MM-dd and dd-MM-yyyy formats
        /// </summary>
        class CustomDateTimeBinder : IBinder
        {
            public object GetValue(CultureInfo culture, string str)
            {
                if (str == null) return null;

                // is it dd-MM-yyyy?
                if (str.IndexOf('-') == 2)
                {
                    return DateTime.ParseExact(str, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                }

                // ok it's yyyy-MM-dd
                return DateTime.ParseExact(str, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            }
        }
    }
}