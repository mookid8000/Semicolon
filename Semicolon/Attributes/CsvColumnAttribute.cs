using System;

namespace Semicolon.Attributes;

/// <summary>
/// Attribute to indicate that a property of a class must be bound to a value from a CSV row
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class CsvColumnAttribute : Attribute
{
    /// <summary>
    /// The name of the CSV header. This is how a specific property gets bound to a specific column in the CSV format.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Binder to use if a) the type of the property does not have a default binder (is not <see cref="int"/>, <see cref="double"/>, <see cref="DateTime"/>, etc.), or b) you want to affect how the value is bound (e.g. use custom <see cref="DateTime"/> format)
    /// </summary>
    public Type Binder { get; set; }

    /// <summary>
    /// Create the attribute
    /// </summary>
    public CsvColumnAttribute(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name), "CSV column attribute must be created with a non-null name, which must correspond to a header in the CSV format");
    }
}