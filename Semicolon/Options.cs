using System;
using System.Collections.Generic;
using System.Globalization;

namespace Semicolon
{
    public class Options
    {
        /// <summary>
        /// Gets the default options
        /// </summary>
        public static Options Default() => new Options();

        /// <summary>
        /// Configures which separator to expect between columns
        /// </summary>
        public char ColumnSeparator { get; set; } = ';';

        /// <summary>
        /// Configures which <see cref="CultureInfo"/> to be used when automatically binding values that accept an <see cref="IFormatProvider"/>
        /// </summary>
        public CultureInfo CultureInfo { get; set; } = CultureInfo.InvariantCulture;

        /// <summary>
        /// When set, it is assumed that the CSV has no headers and the cells come in the order specifiec here
        /// </summary>
        public ICollection<string> Headers { get; } = new List<string>();
    }
}