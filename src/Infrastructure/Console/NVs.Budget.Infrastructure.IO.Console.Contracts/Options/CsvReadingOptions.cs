using System.Text.RegularExpressions;
using FluentResults;

namespace NVs.Budget.Infrastructure.IO.Console.Options;

public class CsvReadingOptions(IDictionary<Regex, CsvFileReadingOptions> options)
{
    public static readonly CsvReadingOptions Empty = new(new Dictionary<Regex, CsvFileReadingOptions>());

    protected readonly IDictionary<Regex, CsvFileReadingOptions> Options = options;

    public IReadOnlyDictionary<Regex, CsvFileReadingOptions> Snapshot => Options.AsReadOnly();

    public Result<SpecificCsvFileReadingOptions> GetFileOptionsFor(string name)
    {
        var key = Options.Keys.FirstOrDefault(k => k.IsMatch(name));
        var value = key is null ? null : Options[key];

        if (value is null)
        {
            return Result.Fail(new UnexpectedFileNameGivenError(name));
        }

        return new SpecificCsvFileReadingOptions(name, value);
    }

    private class UnexpectedFileNameGivenError(string name) : IError
    {
        public string Message => "Reading configuration for this file is not defined!";

        public Dictionary<string, object> Metadata { get; } = new()
        {
            { nameof(name), name }
        };

        public List<IError> Reasons { get; } = new();
    }
}
