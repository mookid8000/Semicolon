using System.Globalization;

namespace Semicolon.Binding;

/// <summary>
/// A "binder" is what parses a value from the CSV format and returns a correponding value to be set on the row type
/// </summary>
public interface IBinder
{
    /// <summary>
    /// Gets the value
    /// </summary>
    /// <param name="culture">Culture configured in the CSV parser</param>
    /// <param name="str">String value from the cell to be parsed</param>
    /// <returns>Must return an <see cref="object"/> as the value to be set on the property of the row type</returns>
    object GetValue(CultureInfo culture, string str);
}