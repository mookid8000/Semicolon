using System;

namespace CsvParser.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class CsvColumnAttribute : Attribute
    {
        public string Name { get; }

        public Type Binder { get; set; }

        public CsvColumnAttribute(string name)
        {
            Name = name;
        }
    }
}