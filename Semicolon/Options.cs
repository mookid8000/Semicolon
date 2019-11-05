using System.Globalization;

namespace Semicolon
{
    public class Options
    {
        public static readonly Options Default = new Options();

        public char ColumnSeparator { get; set; } = ';';

        public CultureInfo CultureInfo { get; set; } = CultureInfo.InvariantCulture;
    }
}