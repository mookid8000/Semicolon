using System;

namespace Semicolon.Exceptions
{
    /// <summary>
    /// Exception thrown when CSV could not be parsed
    /// </summary>
    public class CsvParserException : Exception
    {
        /// <summary>
        /// Creates the exception
        /// </summary>
        public CsvParserException(string message) : base(message)
        {
        }

        /// <summary>
        /// Creates the exception
        /// </summary>
        public CsvParserException(string message, Exception exception) : base(message, exception)
        {
        }
    }
}