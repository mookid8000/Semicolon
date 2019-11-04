# Semicolon

It's a CSV parser. 🙂

Let's say you're handed some CSV like this:

```
Player ID;Name;Duration (s)
734;Joe;65
78439;Moe;63
12342;Bo;67
```

and you want to parse that somehow. This CSV parser can help you. 😁

You start by creating a type with properties for the columns you want to extract (it doesn't have to be all of them):

```csharp
class CsvRow
{
    [CsvColumn("Player ID")]
    public string Id { get; set; }

    [CsvColumn("Name")]
    public string Name { get; set; }

    [CsvColumn("Duration (s)", Binder = typeof(ConvertSecondsToTimeSpanBinder))]
    public TimeSpan Duration { get; set; }
}
```

As you can see, the CSV column name is bound to columns from the CSV text by specifying the column name with the `[CsvColumn(...)]` attribute.

Another thing to note: Since the duration in the CSV text is specified as an integer (the number of seconds), and we want the value as a `TimeSpan`, we make the parser use our own custom binder, `ConvertSecondsToTimeSpanBinder`:

```csharp
class ConvertSecondsToTimeSpanBinder : IBinder
{
    public object GetValue(CultureInfo culture, string str) => TimeSpan.FromSeconds(int.Parse(str));
}
```

Nice. Let's parse the CSV:

```csharp
var parser = new Parser<CsvRow>();

var rows = parser.ParseCsv(CsvText);
```

If we now call `rows.DumpTable()` (extension method from [Testy](https://github.com/rebus-org/Testy)), we get this:

```
+-------+------+----------+
| Id    | Name | Duration |
+-------+------+----------+
| 734   | Joe  | 00:01:05 |
| 78439 | Moe  | 00:01:03 |
| 12342 | Bo   | 00:01:07 |
+-------+------+----------+

```

which is what we hoped for. 😎
