using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvParser.Attributes;
using CsvParser.Binding;
using CsvParser.Exceptions;
using CsvParser.Extensions;
using FastMember;

// ReSharper disable ArgumentsStyleLiteral

namespace CsvParser
{
    public class Parser<TRow>
    {
        readonly TypeAccessor _accessor = TypeAccessor.Create(typeof(TRow));
        readonly string _columnSeparator = ";";
        readonly CultureInfo _defaultCulture;

        public Parser() : this(CultureInfo.InvariantCulture)
        {
        }

        public Parser(CultureInfo defaultCulture)
        {
            _defaultCulture = defaultCulture ?? throw new ArgumentNullException(nameof(defaultCulture));
        }

        public IEnumerable<TRow> ParseCsv(string csv)
        {
            using (var reader = new StringReader(csv))
            {
                foreach (var row in ParseCsv(reader))
                {
                    yield return row;
                }
            }
        }

        public IEnumerable<TRow> ParseCsv(TextReader reader)
        {
            var firstLine = reader.ReadLine() ?? throw new FormatException($"Expected the first line to contain a '{_columnSeparator}'-separated list of column headers");
            var headers = firstLine.Split(new[] { _columnSeparator }, StringSplitOptions.None).Select(text => text.Trim()).ToArray();
            var rowParser = GetRowparser(headers);

            while (true)
            {
                var line = reader.ReadLine();
                if (line == null) yield break;

                var values = line.Split(new[] { _columnSeparator }, StringSplitOptions.None);

                yield return rowParser(values);
            }
        }

        Func<string[], TRow> GetRowparser(string[] headers)
        {
            Member[] GetMembersForHeader(string header) => _accessor.GetMembers()
                .Where(m => m.HasAttribute<CsvColumnAttribute>(c => IsColumnMatch(header, c.Name)))
                .ToArray();

            var members = headers.Select(GetMembersForHeader).ToList();

            var propertiesWithoutCsvColumns = _accessor.GetMembers()
                .Select(m => new
                {
                    Member = m,
                    CsvColumnAttribute = m.GetAttribute<CsvColumnAttribute>()
                })
                .Where(a => a.CsvColumnAttribute != null)
                .Where(a => !headers.Any(nameFromCsvHeader => IsColumnMatch(nameFromCsvHeader, a.CsvColumnAttribute.Name)))
                .ToList();

            if (propertiesWithoutCsvColumns.Any())
            {
                throw new ArgumentException($@"Cannot parse CSV into {typeof(TRow)}, because the CSV data lacks columns for the following properties:

{string.Join(Environment.NewLine, propertiesWithoutCsvColumns.Select(p => $"    {p.Member.Name}: CSV column '{p.CsvColumnAttribute.Name}'"))}

The CSV data contains the following columns:

{string.Join(Environment.NewLine, headers.Select(name => $"    {name}"))}

Please ensure that all CSV columns referenced from the row type can be resolved from the CSV passed to the parser.");
            }

            TRow CreateNew()
            {
                try
                {
                    return (TRow)_accessor.CreateNew();
                }
                catch (Exception exception)
                {
                    throw new ApplicationException($"Could not create instance of {typeof(TRow)}", exception);
                }
            }

            var getters = members
                .Select(memberForHeader =>
                {
                    return memberForHeader
                        .Select(member =>
                        {
                            var customBinderType = member.GetAttribute<CsvColumnAttribute>().Binder;

                            if (customBinderType != null)
                            {
                                return new { Member = member, Binder = GetCustomBinderInstance(customBinderType) };
                            }

                            if (_predefinedBinders.TryGetValue(member.Type, out var predefinedBinder))
                            {
                                return new { Member = member, Binder = GetValueGetter(predefinedBinder, member) };
                            }

                            throw new ArgumentException(
                                $"Sorry, don't know how to bind property {member.Name} of type {member.Type}");
                        })
                        .ToArray();
                })
                .ToArray();

            return values =>
            {
                var row = CreateNew();

                try
                {
                    for (var index = 0; index < values.Length; index++)
                    {
                        var textValue = values[index];
                        var gettersForThisValue = getters[index];

                        foreach (var getter in gettersForThisValue)
                        {
                            var member = getter.Member;
                            var binder = getter.Binder;
                            var name = member.Name;
                            var objectValue = binder(textValue, _defaultCulture);

                            try
                            {
                                _accessor[row, name] = objectValue;
                            }
                            catch (Exception exception)
                            {
                                throw new CsvParserException($"Error when setting property {name} = {textValue}", exception);
                            }
                        }
                    }

                    return row;
                }
                catch (Exception exception)
                {
                    throw new CsvParserException($@"Error when binding CSV values

{string.Join(Environment.NewLine, values.Select(v => $"    {v}"))}

to instance of {typeof(TRow)}", exception);
                }
            };
        }

        static Func<string, CultureInfo, object> GetValueGetter(Func<string, CultureInfo, object> predefinedBinder, Member member)
        {
            return (value, culture) =>
            {
                try
                {
                    return predefinedBinder(value, culture);
                }
                catch (FormatException exception)
                {
                    throw new FormatException(
                        $"Could not parse the value '{value}' into {member.Type} using the culture {culture}",
                        exception);
                }
            };
        }

        static Func<string, CultureInfo, object> GetCustomBinderInstance(Type customBinderType)
        {
            try
            {
                var binder = (IBinder)TypeAccessor.Create(customBinderType).CreateNew();

                return (value, culture) =>
                {
                    try
                    {
                        return binder.GetValue(culture, value);
                    }
                    catch (Exception exception)
                    {
                        throw new FormatException(
                            $"Could not bind value '{value}' using instance of custom binder {binder.GetType()}",
                            exception);
                    }
                };
            }
            catch (Exception exception)
            {
                throw new ArgumentException(
                    $"Could not create custom binder of type {customBinderType}. Please note that custom binders must implement IBinder and have a default constructor to work.",
                    exception);
            }
        }

        readonly Dictionary<Type, Func<string, CultureInfo, object>> _predefinedBinders = new Dictionary<Type, Func<string, CultureInfo, object>>
        {
            [typeof(string)] = (value, culture) => value,

            [typeof(float)] = (value, culture) => float.Parse(value, culture),
            [typeof(double)] = (value, culture) => double.Parse(value, culture),
            [typeof(decimal)] = (value, culture) => decimal.Parse(value, culture),

            [typeof(byte)] = (value, culture) => byte.Parse(value, culture),
            [typeof(short)] = (value, culture) => short.Parse(value, culture),
            [typeof(int)] = (value, culture) => int.Parse(value, culture),
            [typeof(long)] = (value, culture) => long.Parse(value, culture),

            [typeof(DateTime)] = (value, culture) => DateTime.Parse(value, culture),
            [typeof(DateTimeOffset)] = (value, culture) => DateTimeOffset.Parse(value, culture),
        };

        static bool IsColumnMatch(string headerFromCsv, string nameFromAttribute) => nameFromAttribute == headerFromCsv;
    }
}