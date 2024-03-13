using System;
using NUnit.Framework;
using Semicolon.Extensions;
using Testy;

namespace Semicolon.Tests;

[TestFixture]
public class TestStringExtensions : FixtureBase
{
    [Test]
    public void SeeWhatItCanDo()
    {
        const string input = "'v1';'v2'";
        var output = input.SplitDelimited(';', '\'');

        Console.WriteLine($@"

{input}

=>

{string.Join(Environment.NewLine, output)}

");

        Assert.That(output, Is.EqualTo(new[] { "v1", "v2" }));
    }
}