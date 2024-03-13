using NUnit.Framework;
using Testy;
using Testy.Extensions;

namespace Semicolon.Tests;

[TestFixture]
public class TestParser_EdgeCases : FixtureBase
{
    [Test]
    [Description("Number of values in a row exceeds the number of headers")]
    public void MoreValuesThanHeaders()
    {
        var parser = new Parser<Empty>(new Options { ColumnSeparator = ';' });

        const string csv = @"h1;h2
v1;v2;v3";

        var rows = parser.ParseCsv(csv);

        rows.DumpTable();
    }

    class Empty { }
}