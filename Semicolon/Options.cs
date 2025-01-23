using System;
using System.Collections.Generic;
using System.Globalization;

namespace Semicolon;

/// <summary>
/// Represents options for Semicolon :)
/// </summary>
public class Options
{
    /// <summary>
    /// Gets the default options
    /// </summary>
    public static Options Default() => new();

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

    /// <summary>
    /// When set, values are expected to be surrounded by pairs of these. Default is <code>null</code>,
    /// which means that values are expected to not be delimited
    /// </summary>
    public char? ValueDelimiter { get; set; }
}