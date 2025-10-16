using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using CsvHelper;
using CsvHelper.Configuration;
using FluentResults;
using NMoneys;
using NVs.Budget.Application.Contracts.Entities.Budgeting;
using NVs.Budget.Infrastructure.Files.CSV.Contracts;
using NVs.Budget.Infrastructure.Files.CSV.Errors;

namespace NVs.Budget.Infrastructure.Files.CSV;

internal partial class CsvFileReader : ICsvFileReader
{
    private static readonly Regex CellsIndexPattern = GenerateCellIndexPattern();

    public async IAsyncEnumerable<Result<UnregisteredOperation>> ReadUntrackedOperations(
        StreamReader reader, 
        FileReadingSetting config, 
        [EnumeratorCancellation] CancellationToken ct)
    {
        var csvConfig = new CsvConfiguration(config.Culture)
        {
            HasHeaderRecord = false,
            MissingFieldFound = null
        };

        var parser = new CsvParser(reader, csvConfig, true);
        var rowNumber = 0;

        while (await parser.ReadAsync())
        {
            ct.ThrowIfCancellationRequested();
            rowNumber++;

            // Skip empty rows
            if (string.IsNullOrWhiteSpace(parser.RawRecord))
            {
                continue;
            }

            // Validate row
            var validationResult = ValidateRow(parser, config.Validation, rowNumber);
            if (validationResult.IsFailed)
            {
                yield return validationResult.ToResult();
                continue;
            }

            if (!validationResult.Value)
            {
                // Row doesn't meet validation criteria but it's not an error (e.g., it's a header row)
                continue;
            }

            // Parse row
            var operationResult = ParseRow(parser, config, rowNumber);
            yield return operationResult;
        }
    }

    private Result<bool> ValidateRow(IParser parser, IReadOnlyCollection<ValidationRule> validationRules, int rowNumber)
    {
        if (validationRules == null || validationRules.Count == 0)
        {
            return true;
        }

        foreach (var rule in validationRules)
        {
            var valueResult = EvaluatePattern(parser, rule.Pattern);
            if (valueResult.IsFailed)
            {
                return Result.Fail<bool>(new RowNotParsedError(rowNumber, valueResult.Errors));
            }

            var matches = rule.Condition switch
            {
                ValidationRule.ValidationCondition.Equals => valueResult.Value == rule.Value,
                ValidationRule.ValidationCondition.NotEquals => valueResult.Value != rule.Value,
                _ => throw new ArgumentOutOfRangeException(nameof(rule.Condition))
            };

            if (!matches)
            {
                if (!string.IsNullOrEmpty(rule.ErrorMessage))
                {
                    return Result.Fail<bool>(new RowNotParsedError(rowNumber, 
                        new List<IError> { new ValidationFailedError(rule.ErrorMessage) }));
                }
                return false;
            }
        }

        return true;
    }

    private Result<UnregisteredOperation> ParseRow(IParser parser, FileReadingSetting config, int rowNumber)
    {
        // Parse timestamp
        var timestampResult = ParseField(parser, config.Fields, nameof(UnregisteredOperation.Timestamp), 
            s => DateTime.SpecifyKind(DateTime.Parse(s, config.Culture), config.DateTimeKind));
        if (timestampResult.IsFailed)
        {
            return BuildParseError(rowNumber, timestampResult.Errors);
        }

        // Parse currency code
        var currencyResult = ParseField(parser, config.Fields, "Amount.CurrencyCode", Currency.Get);
        if (currencyResult.IsFailed)
        {
            return BuildParseError(rowNumber, currencyResult.Errors);
        }

        // Parse amount
        var moneyResult = ParseField(parser, config.Fields, nameof(UnregisteredOperation.Amount), 
            m => new Money(decimal.Parse(m, NumberStyles.Any, config.Culture), currencyResult.Value));
        if (moneyResult.IsFailed)
        {
            return BuildParseError(rowNumber, moneyResult.Errors);
        }

        // Parse description
        var descriptionResult = ParseField(parser, config.Fields, nameof(UnregisteredOperation.Description), s => s);
        if (descriptionResult.IsFailed)
        {
            return BuildParseError(rowNumber, descriptionResult.Errors);
        }

        // Parse attributes
        var attributesResult = ParseAttributes(parser, config.Attributes);
        if (attributesResult.IsFailed)
        {
            return BuildParseError(rowNumber, attributesResult.Errors);
        }

        return new UnregisteredOperation(
            timestampResult.Value,
            moneyResult.Value,
            descriptionResult.Value,
            attributesResult.Value
        );
    }

    private Result<IReadOnlyDictionary<string, object>?> ParseAttributes(IParser parser, IReadOnlyDictionary<string, string> attributePatterns)
    {
        if (attributePatterns == null || attributePatterns.Count == 0)
        {
            return Result.Ok<IReadOnlyDictionary<string, object>?>(null);
        }

        var attributes = new Dictionary<string, object>();
        
        foreach (var (name, pattern) in attributePatterns)
        {
            var valueResult = EvaluatePattern(parser, pattern);
            if (valueResult.IsFailed)
            {
                return Result.Fail<IReadOnlyDictionary<string, object>?>(new AttributeParsingError(name));
            }

            attributes.Add(name, valueResult.Value);
        }

        return Result.Ok<IReadOnlyDictionary<string, object>?>(attributes);
    }

    private Result<T> ParseField<T>(IParser parser, IReadOnlyDictionary<string, string> fields, string fieldName, Func<string, T> convertFn)
    {
        if (!fields.TryGetValue(fieldName, out var pattern))
        {
            return Result.Fail(new NoFieldOptionsProvidedFor(fieldName));
        }

        var rawValueResult = EvaluatePattern(parser, pattern);
        if (rawValueResult.IsFailed)
        {
            return rawValueResult.ToResult<T>();
        }

        try
        {
            return convertFn(rawValueResult.Value);
        }
        catch (Exception e)
        {
            var error = new ConversionError(e);
            error.Metadata.Add(nameof(fieldName), fieldName);
            return Result.Fail(error);
        }
    }

    private Result<string> EvaluatePattern(IParser parser, string pattern)
    {
        var usedCells = CellsIndexPattern.Matches(pattern)
            .Select(m => (match: m, index: int.Parse(m.Groups[1].Value)))
            .ToList();

        if (usedCells.Count == 0)
        {
            // No placeholders, return the pattern as-is
            return pattern;
        }

        try
        {
            var values = usedCells
                .Select(m => m.index)
                .Distinct()
                .Select(i => (index: i, value: parser[i]))
                .ToDictionary(v => v.index, v => v.value);

            var matchIndex = 0;
            var strpos = 0;
            var builder = new StringBuilder();

            while (matchIndex < usedCells.Count)
            {
                var match = usedCells[matchIndex];
                
                // Append any text before the placeholder
                if (strpos < match.match.Index)
                {
                    builder.Append(pattern, strpos, match.match.Index - strpos);
                    strpos = match.match.Index;
                }

                // Append the cell value
                builder.Append(values[match.index]);
                strpos += match.match.Length;
                matchIndex++;
            }

            // Append any remaining text after the last placeholder
            if (strpos < pattern.Length)
            {
                builder.Append(pattern, strpos, pattern.Length - strpos);
            }

            return builder.ToString();
        }
        catch (Exception e)
        {
            var error = new ConversionError(e);
            error.Metadata.Add("Pattern", pattern);
            return Result.Fail(error);
        }
    }

    private Result<UnregisteredOperation> BuildParseError(int rowNumber, List<IError> errors)
    {
        return Result.Fail<UnregisteredOperation>(new RowNotParsedError(rowNumber, errors));
    }

    [GeneratedRegex(@"\{(\d+)\}", RegexOptions.Compiled)]
    private static partial Regex GenerateCellIndexPattern();
}

