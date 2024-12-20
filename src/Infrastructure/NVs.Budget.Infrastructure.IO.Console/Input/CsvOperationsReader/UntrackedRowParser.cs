using System.Text;
using System.Text.RegularExpressions;
using CsvHelper;
using FluentResults;
using NMoneys;
using NVs.Budget.Application.Contracts.Entities.Budgeting;
using NVs.Budget.Controllers.Console.Contracts.Errors;
using NVs.Budget.Domain.Extensions;
using NVs.Budget.Infrastructure.IO.Console.Input.CsvOperationsReader.Errors;
using NVs.Budget.Infrastructure.IO.Console.Input.Errors;
using NVs.Budget.Infrastructure.IO.Console.Options;

namespace NVs.Budget.Infrastructure.IO.Console.Input.CsvOperationsReader;

internal partial class UntrackedRowParser(IParser parser, SpecificCsvFileReadingOptions fileOptions, CancellationToken ct)
{
    private static readonly Regex CellsIndexPattern = GenerateCellIndexPattern();
    private volatile int _row = -1;
    public async Task<bool> ReadAsync()
    {
        ct.ThrowIfCancellationRequested();

        var hasRow = await parser.ReadAsync();
        Interlocked.Increment(ref _row);

        return hasRow;
    }

    public Result<bool> IsRowValid()
    {
        if (string.IsNullOrWhiteSpace(parser.RawRecord))
        {
            return false;
        }

        if (fileOptions.ValidationRules is { Count: > 0 })
        {
            foreach (var (key,rule) in fileOptions.ValidationRules)
            {
                var value = ReadRaw(rule.FieldConfiguration);
                if (value.IsFailed) return value.ToResult();

                switch (rule.Condition)
                {
                    case ValidationRule.ValidationCondition.Equals:
                        return value.Value.Equals(rule.Value);

                    case ValidationRule.ValidationCondition.NotEquals:
                        return !value.Value.Equals(rule.Value);

                    default:
                        throw new ArgumentOutOfRangeException(nameof(rule.Condition));
                }
            }
        }

        return true;
    }

    public Result<UnregisteredOperation> GetRow()
    {
        var timestampResult = ReadField(nameof(UnregisteredOperation.Timestamp), s => DateTime.SpecifyKind(DateTime.Parse(s), fileOptions.DateTimeKind));
        if (timestampResult.IsFailed)
        {
            return BuildParseError(timestampResult.Errors);
        }

        var currencyResult = ReadField(nameof(UnregisteredOperation.Amount.CurrencyCode), Currency.Get);
        if (currencyResult.IsFailed)
        {
            return BuildParseError(currencyResult.Errors);
        }

        var moneyResult = ReadField(nameof(UnregisteredOperation.Amount), m => new Money(decimal.Parse(m, fileOptions.CultureInfo), currencyResult.Value));
        if (moneyResult.IsFailed)
        {
            return BuildParseError(moneyResult.Errors);
        }

        var descriptionResult = ReadField(nameof(UnregisteredOperation.Description), s => s);
        if (descriptionResult.IsFailed)
        {
            return BuildParseError(descriptionResult.Errors);
        }

        var attributesResult = ReadAttributes();
        if (attributesResult.IsFailed)
        {
            return BuildParseError(attributesResult.Errors);
        }

        return new UnregisteredOperation(
            timestampResult.Value,
            moneyResult.Value,
            descriptionResult.Value,
            attributesResult.Value
        );
    }

    private Result<IReadOnlyDictionary<string, object>?> ReadAttributes()
    {
        var attributes = new Dictionary<string, object>();
        var attributesOptions = fileOptions.Attributes ?? new Dictionary<string, FieldConfiguration>();
        foreach (var (name, config) in attributesOptions)
        {
            var value = ReadRaw(config);
            if (value.IsFailed)
            {
                return Result.Fail(new AttributeParsingError(name));
            }

            attributes.Add(name, value.Value);
        }

        return attributes.AsReadOnly();
    }

    private Result<T> ReadField<T>(string fieldName, Func<string, T> convertFn)
    {
        var fileOption = fileOptions[fieldName];
        if (fileOption is null)
        {
            return Result.Fail(new NoFieldOptionsProvidedFor(fieldName));
        }

        var rawValue = ReadRaw(fileOption);
        if (rawValue.IsFailed)
        {
            return rawValue.ToResult<T>();
        }

        try
        {
            return convertFn(rawValue.Value);
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionBasedError(e).WithMetadata(nameof(fieldName), fieldName));
        }
    }

    private Result<string> ReadRaw(FieldConfiguration fileOption)
    {
        var usedCells = CellsIndexPattern.Matches(fileOption.Pattern).Select(m => (match: m, index: int.Parse(m.Groups[1].Value))).ToList();
        if (usedCells.Count > 0)
        {
            var values = usedCells.Select(m => m.index).Distinct().Select(i => (index: i, value: parser[i])).ToDictionary(v => v.index, v => v.value);

            var matchIndex = 0;
            var strpos = 0;
            var builder = new StringBuilder();
            while (matchIndex < usedCells.Count)
            {
                var match = usedCells[matchIndex];
                if (strpos < match.match.Index)
                {
                    builder.Append(fileOption.Pattern, strpos, match.match.Index - strpos);
                    strpos = match.match.Index;
                }

                builder.Append(values[match.index]);
                strpos += match.match.Length;
                matchIndex++;
            }

            if (strpos < fileOption.Pattern.Length)
            {
                builder.Append(fileOption.Pattern, strpos, fileOption.Pattern.Length - strpos);
            }

            return builder.ToString();
        }

        return fileOption.Pattern;
    }

    private Result<UnregisteredOperation> BuildParseError(List<IError> errors) => Result.Fail<UnregisteredOperation>(new RowNotParsedError(_row, errors).WithMetadata(nameof(fileOptions.FileName), fileOptions.FileName));

    [GeneratedRegex("{(\\d+)}", RegexOptions.Compiled)]
    private static partial Regex GenerateCellIndexPattern();
}
