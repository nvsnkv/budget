using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using NVs.Budget.Infrastructure.IO.Console.Options;

namespace NVs.Budget.Infrastructure.IO.Console.Output.Budgets;

public class YamlBasedCsvReadingOptionsWriter(IOutputStreamProvider streams, IOptionsSnapshot<OutputOptions> options) : IObjectWriter<CsvReadingOptions>
{
    private static readonly Regex UnsafeCharsPattern = new("[{} ]", RegexOptions.Compiled);

    public Task Write(CsvReadingOptions criterion, CancellationToken ct) => Write(criterion, options.Value.OutputStreamName, ct);
    public async Task Write(CsvReadingOptions criterion, string streamName, CancellationToken ct)
    {
        var writer = await streams.GetOutput(streamName);
        await writer.WriteLineAsync("CsvReadingOptions:");
        foreach (var (pattern, fileOpts) in criterion.Snapshot)
        {
            await writer.WriteLineAsync($"  {Encode(pattern.ToString())}:");
            await writer.WriteLineAsync("    CultureCode: " + fileOpts.CultureInfo.Name);
            await writer.WriteLineAsync("    DateTimeKind: " + fileOpts.DateTimeKind);
            foreach (var (field, config) in fileOpts)
            {
                await writer.WriteLineAsync($"    {Encode(field)}: {Encode(config.Pattern)}");
            }

            await writer.WriteLineAsync("    Attributes:");
            if (fileOpts.Attributes is not null)
            {
                foreach (var (name, config) in fileOpts.Attributes)
                {
                    await writer.WriteLineAsync($"      {Encode(name)}: {Encode(config.Pattern)}");
                }
            }

            await writer.WriteLineAsync("    ValidationRules:");
            if (fileOpts.ValidationRules is not null)
            {
                foreach (var (name,rule) in fileOpts.ValidationRules)
                {
                    await writer.WriteLineAsync($"      {Encode(name)}:");
                    await writer.WriteLineAsync($"        FieldConfiguration: {Encode(rule.FieldConfiguration.Pattern)}");
                    await writer.WriteLineAsync($"        Condition: {Encode(rule.Condition.ToString())}");
                    await writer.WriteLineAsync($"        Value: {Encode(rule.Value)}");
                }
            }
        }

        await writer.FlushAsync(ct);
    }

    private string Encode(string value)
    {
        if (UnsafeCharsPattern.IsMatch(value))
        {
            return $"\"{value.Replace("\"", "\\\"")}\"";
        }

        return value;
    }

    public Task Write(IEnumerable<CsvReadingOptions> collection, CancellationToken ct) => Write(collection, options.Value.OutputStreamName, ct);
    public async Task Write(IEnumerable<CsvReadingOptions> collection, string streamName, CancellationToken ct)
    {
        foreach (var option in collection)
        {
            await Write(option, streamName, ct);
        }
    }
}
