using System;

namespace CsvParser.Exceptions
{
    public class CsvParserException : Exception
    {
        public CsvParserException(string message) : base(message)
        {
        }

        public CsvParserException(string message, Exception exception) : base(message, exception)
        {
        }
    }
}