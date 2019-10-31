using System;
using System.Collections.Generic;
using System.Globalization;

namespace CsvParser.Binding
{
    public class Predefined
    {
        static readonly Dictionary<Type, IBinder> Instances = new Dictionary<Type, IBinder>
        {
            [typeof(string)] = new Predefined.StringBinder(),
            [typeof(byte)] = new Predefined.ByteBinder(),
            [typeof(short)] = new Predefined.ShortBinder(),
            [typeof(int)] = new Predefined.IntBinder(),
            [typeof(long)] = new Predefined.LongBinder(),
            [typeof(float)] = new Predefined.FloatBinder(),
            [typeof(double)] = new Predefined.DoubleBinder(),
            [typeof(decimal)] = new Predefined.DecimalBinder(),
            [typeof(DateTime)] = new Predefined.DateTimeBinder(),
            [typeof(DateTimeOffset)] = new Predefined.DateTimeOffsetBinder(),
            [typeof(TimeSpan)] = new Predefined.TimeSpanBinder(),
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
    }
}