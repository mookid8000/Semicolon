using System.Globalization;

namespace CsvParser.Binding
{
    public interface IBinder
    {
        object GetValue(CultureInfo culture, string str);
    }
}