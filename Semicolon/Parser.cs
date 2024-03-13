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

namespace Semicolon;

/// <summary>
/// This is the main parser class. New this one up, passing as <typeparamref name="TRow"/>  the type you want to represent each row of data to extract.
/// Please note that the type does not have to bind to ALL columns of the CSV data, but all columns (configured with <see cref="CsvColumnAttribute"/>
/// on properties with public setters) MUST be resolveable from the column name configured.
/// </summary>
public class Parser<TRow>
{
    readonly Options _options;
    readonly TypeAccessor _accessor = TypeAccessor.Create(typeof(TRow));

    /// <summary>
    /// Creates the parser with default options
    /// </summary>
    public Parser() : this(null)
    {
    }

    /// <summary>
    /// Creates the CSV parser using the given <paramref name="options"/>. If no options are given, <see cref="Options.Default"/> will be used
    /// </summary>
    public Parser(Options options) => _options = options ?? Options.Default();

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

        var headers = GetHeaders(textReader);
        var rowParser = GetRowparser(headers);

        while (true)
        {
            var line = textReader.ReadLine();
            if (line == null) yield break;

            var values = SplitLine(line);

            yield return rowParser(values);
        }
    }

    string[] GetHeaders(TextReader textReader)
    {
        if (_options.Headers.Any()) return _options.Headers.ToArray();

        var line = textReader.ReadLine()
                   ?? throw new FormatException($"Expected the first line to contain a '{_options.ColumnSeparator}'-separated list of column headers");

        if (string.IsNullOrWhiteSpace(line))
        {
            throw new FormatException("The first line of the CSV text was empty. Please make the necessary trimming of the CSV text, before passing it to the parser");
        }

        var headers = SplitLine(line)
            .Select(text => text.Trim())
            .ToArray();

        return headers;
    }

    string[] SplitLine(string line)
    {
        if (_options.ValueDelimiter != null)
        {
            // values are delimited! use splitter!
            return line.SplitDelimited(_options.ColumnSeparator, _options.ValueDelimiter.Value);
        }

        var values = line.Split([_options.ColumnSeparator], StringSplitOptions.None);

        return values;
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
                var maxIndex = Math.Min(values.Length, getters.Length);

                for (var index = 0; index < maxIndex; index++)
                {
                    var textValue = values[index];
                    var gettersForThisValue = getters[index];

                    foreach (var getter in gettersForThisValue)
                    {
                        var member = getter.Member;
                        var binder = getter.Binder;
                        var name = member.Name;
                        var objectValue = binder(textValue, _options.CultureInfo);

                        try
                        {
                            _accessor[row, name] = objectValue;
                        }
                        catch (ArgumentOutOfRangeException exception) when (exception.ParamName == "name")
                        {
                            throw new CsvParserException($@"Error when setting property {name} = {textValue}

This is probably an indication that the property does not have a public setter.

Bound properties must have a public setter, so Semicolon can set their values.", exception);
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
            return (IBinder)TypeAccessor.Create(customBinderType).CreateNew();
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