using System;

namespace Semicolon.Extensions
{
    static class StringExtensions
    {
        public static string TrimOne(this string str, char c)
        {
            var value = c.ToString();

            if (!str.StartsWith(value)) throw new FormatException($"Could not trim '{c}' from beginning of string '{str}'");
            if (!str.EndsWith(value)) throw new FormatException($"Could not trim '{c}' from end of string '{str}'");

            return str.Substring(1, str.Length - 2);
        }
    }
}