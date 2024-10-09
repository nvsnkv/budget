using System.Text.RegularExpressions;
using NVs.Budget.Infrastructure.IO.Console.Input.Options;

namespace NVs.Budget.Infrastructure.IO.Console.Options;

public class CsvReadingOptions(IDictionary<Regex, CsvFileReadingOptions> options)
{
    protected readonly IDictionary<Regex, CsvFileReadingOptions> Options = options;

    public IReadOnlyDictionary<Regex, CsvFileReadingOptions> Snapshot => Options.AsReadOnly();

    public CsvFileReadingOptions? GetFileOptionsFor(string name)
    {
        var key = Options.Keys.FirstOrDefault(k => k.IsMatch(name));
        return key is null ? null : Options[key];
    }
}
