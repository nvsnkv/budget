using System.Globalization;
using System.Text.RegularExpressions;
using FluentResults;
using NVs.Budget.Domain.Extensions;
using NVs.Budget.Infrastructure.IO.Console.Input.Criteria.Logbook;
using NVs.Budget.Infrastructure.IO.Console.Options;
using YamlDotNet.RepresentationModel;

namespace NVs.Budget.Infrastructure.IO.Console.Input;

internal class YamlBasedCsvReadingOptionsReader : YamlReader, ICsvReadingOptionsReader
{
    private static readonly YamlScalarNode RootKey = new("CsvReadingOptions");
    private static readonly YamlScalarNode CultureCodeKey = new("CultureCode");
    private static readonly YamlScalarNode DateTimeKindKey = new("DateTimeKind");
    private static readonly YamlScalarNode AttributesKey = new("Attributes");
    private static readonly YamlScalarNode ValidationRulesKey = new("ValidationRules");
    private static readonly YamlScalarNode FieldConfigurationKey = new("FieldConfiguration");
    private static readonly YamlScalarNode ConditionKey = new("Condition");
    private static readonly YamlScalarNode ValueKey = new("Value");

    public Task<Result<CsvReadingOptions>> ReadFrom(StreamReader reader, CancellationToken ct) => Task.FromResult(ReadSync(reader));

    private Result<CsvReadingOptions> ReadSync(StreamReader reader)
    {
        var mapResult = LoadRootNodeFrom(reader);
        if (!mapResult.IsSuccess)
        {
            return mapResult.ToResult();
        }

        var mapping = mapResult.Value;

        if (!mapping.Children.TryGetValue(RootKey, out var filesNode) || filesNode is not YamlMappingNode files)
        {
            return Result.Fail(new YamlParsingError("CsvReadingOptions node is not found or not a mapping", [RootKey.ToString()]));
        }

        var fileConfigs = new Dictionary<Regex, CsvFileReadingOptions>();
        var errors = new List<IError>();

        foreach (var (key, value) in files)
        {
            var filePattern = ReadString(key, [RootKey.ToString()]);
            if (filePattern.IsFailed)
            {
                errors.AddRange(filePattern.Errors);
                continue;
            }

            ICollection<string> path = [RootKey.ToString(), filePattern.Value];

            if (value is not YamlMappingNode file)
            {
                errors.Add(new UnexpectedNodeTypeError(value.GetType(), typeof(YamlMappingNode), path));
                continue;
            }

            Regex patten;
            try
            {
                patten = new Regex(filePattern.Value);
            }
            catch (Exception e)
            {
                errors.Add(new YamlParsingError("Invalid regex given", path).WithMetadata("Exception", e));
                continue;
            }

            var fileResult = ReadFile(file, path);
            errors.AddRange(fileResult.Errors);
            if (fileResult.IsSuccess)
            {
                fileConfigs.Add(patten,fileResult.Value);
            }
        }

        return fileConfigs.Any()
            ? Result.Ok(new CsvReadingOptions(fileConfigs)).WithReasons(errors)
            : Result.Fail<CsvReadingOptions>(errors);
    }

    private Result<CsvFileReadingOptions> ReadFile(YamlMappingNode file, ICollection<string> path)
    {
        CultureInfo? cultureCode = null;
        DateTimeKind kind = DateTimeKind.Local;
        var errors = new List<IError>();
        IDictionary<string, FieldConfiguration>? attributes = null;
        IDictionary<string, ValidationRule>? validation = null;
        var fields = new Dictionary<string, FieldConfiguration>();

        foreach (var (key, value) in file)
        {
            if (CultureCodeKey.Equals(key))
            {
                var r = ReadString(value, [..path, CultureCodeKey.ToString()]);
                if (r.IsSuccess)
                {
                    cultureCode = CultureInfo.GetCultureInfo(r.Value);
                }
                else
                {
                    errors.AddRange(r.Errors);
                }
            }
            else if (DateTimeKindKey.Equals(key))
            {
                var r = ReadString(value, [..path, DateTimeKindKey.ToString()]);
                if (r.IsFailed)
                {
                    errors.AddRange(r.Errors);
                }
                else
                {
                    if (!Enum.TryParse(r.Value, out kind))
                    {
                        errors.Add(new YamlParsingError("Failed to parse DateTimeKind value", [..path, DateTimeKindKey.ToString()]));
                    }
                }
            }
            else if (AttributesKey.Equals(key))
            {
                if (value is not YamlMappingNode attrs)
                {
                    errors.Add(new UnexpectedNodeTypeError(value.GetType(), typeof(YamlMappingNode), [..path, AttributesKey.ToString()]));
                }
                else
                {
                    var r = ReadAttributes(attrs, [..path, AttributesKey.ToString()]);
                    errors.AddRange(r.Errors);
                    if (r.IsSuccess)
                    {
                        attributes = r.Value;
                    }
                }
            }
            else if (ValidationRulesKey.Equals(key))
            {
                if (value is not YamlMappingNode attrs)
                {
                    errors.Add(new UnexpectedNodeTypeError(value.GetType(), typeof(YamlMappingNode), [..path, ValidationRulesKey.ToString()]));
                }
                else
                {
                    var r = ReadValidationRules(attrs, [..path, ValidationRulesKey.ToString()]);
                    errors.AddRange(r.Errors);
                    if (r.IsSuccess)
                    {
                        validation = r.Value;
                    }
                }
            }
            else
            {
                var keyResult = ReadString(key, [..path]);
                if (keyResult.IsSuccess)
                {
                    var valResult = ReadString(value, [..path, keyResult.Value]);
                    if (valResult.IsSuccess)
                    {
                        fields.Add(keyResult.Value, new(valResult.Value));
                    }
                    else
                    {
                        errors.AddRange(valResult.Errors);
                    }
                }
                else
                {
                    errors.AddRange(keyResult.Errors);
                }
            }
        }

        return fields.Any()
            ? Result.Ok(new CsvFileReadingOptions(fields, cultureCode ?? CultureInfo.CurrentCulture, kind, attributes?.AsReadOnly(), validation?.AsReadOnly()))
                .WithErrors(errors)
            : Result.Fail<CsvFileReadingOptions>(errors);
    }

    private Result<Dictionary<string, FieldConfiguration>?> ReadAttributes(YamlMappingNode attrs, ICollection<string> path)
    {
        if (!attrs.Any())
        {
            return Result.Ok<Dictionary<string, FieldConfiguration>?>(null);
        }

        var fields = new Dictionary<string, FieldConfiguration>();
        var errors = new List<IError>();

        foreach (var (key, value) in attrs)
        {
            var keyResult = ReadString(key, [..path]);
            if (keyResult.IsSuccess)
            {
                var valResult = ReadString(value, [..path, keyResult.Value]);
                if (valResult.IsSuccess)
                {
                    fields.Add(keyResult.Value, new(valResult.Value));
                }
                else
                {
                    errors.AddRange(valResult.Errors);
                }
            }
            else
            {
                errors.AddRange(keyResult.Errors);
            }
        }

        return fields.Any()
            ? Result.Ok((Dictionary<string, FieldConfiguration>?)fields).WithErrors(errors)
            : Result.Fail(errors);
    }

    private Result<Dictionary<string, ValidationRule>?> ReadValidationRules(YamlMappingNode attrs, ICollection<string> path)
    {
        if (!attrs.Any())
        {
            return Result.Ok<Dictionary<string, ValidationRule>?>(null);
        }

        var rules = new Dictionary<string, ValidationRule>();
        var errors = new List<IError>();

        foreach (var (key,value) in attrs)
        {
            var keyResult = ReadString(key, path);
            errors.AddRange(keyResult.Errors);
            if (keyResult.IsFailed)
            {
                continue;
            }

            var valResult = ReadValidationRule(value, [..path, keyResult.Value]);
            errors.AddRange(valResult.Errors);
            if (valResult.IsSuccess)
            {
                rules.Add(keyResult.Value, valResult.Value);
            }
        }

        return rules.Any()
            ? Result.Ok((Dictionary<string, ValidationRule>?)rules).WithErrors(errors)
            : Result.Fail(errors);
    }

    private Result<ValidationRule> ReadValidationRule(YamlNode node, ICollection<string> path)
    {
        if (node is not YamlMappingNode mapping)
        {
            return Result.Fail(new UnexpectedNodeTypeError(node.GetType(), typeof(YamlMappingNode), path));
        }

        if (!mapping.Children.TryGetValue(FieldConfigurationKey, out var fieldConfigNode))
        {
            return Result.Fail(new YamlParsingError($"Attribute {FieldConfigurationKey} not found", path));
        }

        if (!mapping.Children.TryGetValue(ConditionKey, out var conditionNode))
        {
            return Result.Fail(new YamlParsingError($"Attribute {ConditionKey} not found", path));
        }

        if (!mapping.Children.TryGetValue(ValueKey, out var valueNode))
        {
            return Result.Fail(new YamlParsingError($"Attribute {ValueKey} not found", path));
        }

        var fieldConfig = ReadString(fieldConfigNode, [..path, FieldConfigurationKey.ToString()]);
        if (fieldConfig.IsFailed)
        {
            return fieldConfig.ToResult();
        }

        var conditionText = ReadString(conditionNode, [..path, ConditionKey.ToString()]);
        if (conditionText.IsFailed)
        {
            return conditionText.ToResult();
        }

        if (!Enum.TryParse(conditionText.Value, out ValidationRule.ValidationCondition condition))
        {
            return Result.Fail(
                new YamlParsingError("Unexpected ValidationCondition value given", [..path, ValidationRulesKey.ToString()]).WithMetadata("Value", conditionText)
            );
        }

        var value = ReadString(valueNode, [..path, ValueKey.ToString()]);
        if (value.IsFailed)
        {
            return value.ToResult();
        }

        return new ValidationRule(new(fieldConfig.Value), condition, value.Value);
    }
}
