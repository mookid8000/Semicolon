using System;
using System.Collections.Generic;
using System.Globalization;

namespace Semicolon.Binding;

/// <summary>
/// Contains all the predefined binders
/// </summary>
class Predefined
{
    static readonly Dictionary<Type, IBinder> Instances = new Dictionary<Type, IBinder>
    {
        [typeof(string)] = new StringBinder(),
        [typeof(byte)] = new ByteBinder(),
        [typeof(short)] = new ShortBinder(),
        [typeof(int)] = new IntBinder(),
        [typeof(long)] = new LongBinder(),
        [typeof(float)] = new FloatBinder(),
        [typeof(double)] = new DoubleBinder(),
        [typeof(decimal)] = new DecimalBinder(),
        [typeof(DateTime)] = new DateTimeBinder(),
        [typeof(DateTimeOffset)] = new DateTimeOffsetBinder(),
        [typeof(TimeSpan)] = new TimeSpanBinder(),
        [typeof(TimeZoneInfo)] = new TimeZoneInfoBinder(),
        [typeof(CultureInfo)] = new CultureInfoBinder(),
    };

    public static IBinder GetBinderOrNull(Type type) => Instances.TryGetValue(type, out var result) ? result : null;

    public class StringBinder : IBinder
    {
        public object GetValue(CultureInfo culture, string str) => str;
    }

    public class ByteBinder : IBinder
    {
        public object GetValue(CultureInfo culture, string str) => byte.Parse(str, culture);
    }

    public class ShortBinder : IBinder
    {
        public object GetValue(CultureInfo culture, string str) => short.Parse(str, culture);
    }

    public class IntBinder : IBinder
    {
        public object GetValue(CultureInfo culture, string str) => int.Parse(str, culture);
    }

    public class LongBinder : IBinder
    {
        public object GetValue(CultureInfo culture, string str) => long.Parse(str, culture);
    }

    public class FloatBinder : IBinder
    {
        public object GetValue(CultureInfo culture, string str) => float.Parse(str, culture);
    }

    public class DoubleBinder : IBinder
    {
        public object GetValue(CultureInfo culture, string str) => double.Parse(str, culture);
    }

    public class DecimalBinder : IBinder
    {
        public object GetValue(CultureInfo culture, string str) => decimal.Parse(str, culture);
    }

    public class DateTimeBinder : IBinder
    {
        public object GetValue(CultureInfo culture, string str) => DateTime.Parse(str, culture);
    }

    public class DateTimeOffsetBinder : IBinder
    {
        public object GetValue(CultureInfo culture, string str) => DateTimeOffset.Parse(str, culture);
    }

    public class TimeSpanBinder : IBinder
    {
        public object GetValue(CultureInfo culture, string str) => TimeSpan.Parse(str, culture);
    }

    public class TimeZoneInfoBinder : IBinder
    {
        public object GetValue(CultureInfo culture, string str) => TimeZoneInfo.FindSystemTimeZoneById(str);
    }

    public class CultureInfoBinder : IBinder
    {
        public object GetValue(CultureInfo culture, string str) => new CultureInfo(str);
    }
}