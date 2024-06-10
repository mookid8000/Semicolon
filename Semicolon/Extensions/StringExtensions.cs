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
            var counter = -1;

            foreach (var c in str)
            {
                counter++;

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

                throw new FormatException($@"Could not split the line

{str}
{new string(' ', counter)}^

into individual values, because value delimiters were not properly balanced.

Here's an example on how lines were expected to be formatted, given the current configuration:

{delimiter}value1{delimiter}{separator}{delimiter}value2{delimiter}{separator}{delimiter}value3{delimiter}

(using '{separator}' as the separator and '{delimiter}' as the value delimiter)

The error occurred at position {counter} in the line ('{c}') as indicated by the ^ above.");
            }

            yield return builder.ToString();
        }

        return MunchThroughIt().ToArray();
    }
}