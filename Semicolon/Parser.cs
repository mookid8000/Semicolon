using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using FastMember;
using Semicolon.Attributes;
using Semicolon.Binding;
using Semicolon.Exceptions;
using Semicolon.Extensions;
// ReSharper disable ArgumentsStyleLiteral

namespace Semicolon
{
    /// <summary>
    /// This is the main parser class. New this one up, passing as <typeparamref name="TRow"/>  the type you want to represent each row of data to extract.
    /// Please note that the type does not have to bind to ALL columns of the CSV data, but all columns (configured with <see cref="CsvColumnAttribute"/>
    /// on properties with public setters) MUST be resolveable from the column name configured.
    /// </summary>
    public class Parser<TRow>
    {
        readonly TypeAccessor _accessor = TypeAccessor.Create(typeof(TRow));
        readonly string _columnSeparator = ";";
        readonly CultureInfo _defaultCulture;

        /// <summary>
        /// Creates the CSV parser with <see cref="CultureInfo.InvariantCulture"/> as the default culture.
        /// </summary>
        public Parser() : this(CultureInfo.InvariantCulture)
        {
        }

        /// <summary>
        /// Creates the CSV parser, using <paramref name="culture"/> as the culture, which gets passed to the binders when parsing values.
        /// </summary>
        public Parser(CultureInfo culture)
        {
            _defaultCulture = culture ?? throw new ArgumentNullException(nameof(culture));
        }

        /// <summary>
        /// Parses the given CSV string
        /// </summary>
        public IEnumerable<TRow> ParseCsv(string csv)
        {
            if (csv == null) throw new ArgumentNullException(nameof(csv));

            using (var reader = new StringReader(csv))
            {
                foreach (var row in ParseCsv(reader))
                {
                    yield return row;
                }
            }
        }

        /// <summary>
        /// Parses CSV from the given <paramref name="textReader"/>
        /// </summary>
        public IEnumerable<TRow> ParseCsv(TextReader textReader)
        {
            if (textReader == null) throw new ArgumentNullException(nameof(textReader));

            var firstLine = textReader.ReadLine() ?? throw new FormatException($"Expected the first line to contain a '{_columnSeparator}'-separated list of column headers");
            var headers = firstLine.Split(new[] { _columnSeparator }, StringSplitOptions.None).Select(text => text.Trim()).ToArray();
            var rowParser = GetRowparser(headers);

            while (true)
            {
                var line = textReader.ReadLine();
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

                            var binder = customBinderType != null
                                ? GetCustomBinderInstance(customBinderType)
                                : Predefined.GetBinderOrNull(member.Type) 
                                  ?? throw new ArgumentException($"Sorry, don't know how to bind property {member.Name} of type {member.Type}");

                            return new { Member = member, Binder = GetValueGetter(binder, member) };
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

        static Func<string, CultureInfo, object> GetValueGetter(IBinder binder, Member member)
        {
            return (value, culture) =>
            {
                try
                {
                    return binder.GetValue(culture, value);
                }
                catch (FormatException exception)
                {
                    throw new FormatException(
                        $"Could not parse the value '{value}' into {member.Type} using the culture {culture}",
                        exception);
                }
            };
        }

        static IBinder GetCustomBinderInstance(Type customBinderType)
        {
            try
            {
                var binder = (IBinder)TypeAccessor.Create(customBinderType).CreateNew();

                return binder;
                //return (value, culture) =>
                //{
                //    try
                //    {
                //        return binder.GetValue(culture, value);
                //    }
                //    catch (Exception exception)
                //    {
                //        throw new FormatException(
                //            $"Could not bind value '{value}' using instance of custom binder {binder.GetType()}",
                //            exception);
                //    }
                //};
            }
            catch (Exception exception)
            {
                throw new ArgumentException(
                    $"Could not create custom binder of type {customBinderType}. Please note that custom binders must implement IBinder and have a default constructor to work.",
                    exception);
            }
        }

        static bool IsColumnMatch(string headerFromCsv, string nameFromAttribute) => nameFromAttribute == headerFromCsv;
    }
}