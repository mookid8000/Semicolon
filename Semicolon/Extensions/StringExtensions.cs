using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Semicolon.Extensions;

static class StringExtensions
{
    public static string TrimOne(this string str, char c)
    {
        var value = c.ToString();

        if (!str.StartsWith(value)) throw new FormatException($"Could not trim '{c}' from beginning of string '{str}'");
        if (!str.EndsWith(value)) throw new FormatException($"Could not trim '{c}' from end of string '{str}'");

        return str.Substring(1, str.Length - 2);
    }

    public static string[] SplitDelimited(this string str, char separator, char delimiter)
    {
        IEnumerable<string> MunchThroughIt()
        {
            var builder = new StringBuilder();
            var isParsingValue = false;

            foreach (var c in str)
            {
                if (!isParsingValue && c == delimiter)
                {
                    isParsingValue = true;
                    continue;
                }

                if (isParsingValue && c != delimiter)
                {
                    builder.Append(c);
                    continue;
                }

                if (isParsingValue && c == delimiter)
                {
                    isParsingValue = false;
                    continue;
                }

                if (!isParsingValue && c == separator)
                {
                    yield return builder.ToString();
                    builder.Clear();
                    continue;
                }

                throw new FormatException($"Cannot");
            }

            yield return builder.ToString();
        }

        return MunchThroughIt().ToArray();

        return str.Split(separator).Select(v => v.TrimOne(delimiter)).ToArray();
    }
}