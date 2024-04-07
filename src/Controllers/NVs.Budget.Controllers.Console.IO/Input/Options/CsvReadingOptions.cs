using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;

namespace NVs.Budget.Controllers.Console.IO.Input.Options;

internal class CsvReadingOptions
{
    private Dictionary<Regex, CsvFileReadingOptions> _options = new ();

    public void UpdateFromConfiguration(IConfiguration configuration)
    {
        var fileConfigs = configuration.GetSection(nameof(CsvReadingOptions)).GetChildren();
        _options = fileConfigs.ToDictionary(c => new Regex(c.Key), CreateFileReadingOptions);
    }
    public CsvFileReadingOptions? GetFileOptionsFor(string name)
    {
        var key = _options.Keys.FirstOrDefault(k => k.IsMatch(name));
        return key is null ? null : _options[key];
    }

    private CsvFileReadingOptions CreateFileReadingOptions(IConfigurationSection section)
    {
        var fields = section.GetChildren().ToDictionary(
            c => c.Key,
            c => new FieldConfiguration(c.Value ?? string.Empty)
        );

        var attributes = section.GetSection(nameof(CsvFileReadingOptions.Attributes)).GetChildren().ToDictionary(
            c => c.Key,
            c => new FieldConfiguration(c.Value ?? string.Empty)
        );

        var validationRules = section.GetSection(nameof(CsvFileReadingOptions.ValidationRules)).GetChildren().ToDictionary(
            c => c.Key,
            CreateValidationRule
        );

        return new CsvFileReadingOptions(fields, attributes, validationRules);
    }

    private ValidationRule CreateValidationRule(IConfigurationSection section)
    {
        var pattern = section.GetValue<string>(nameof(ValidationRule.FieldConfiguration)) ?? string.Empty;
        return new ValidationRule(
            new FieldConfiguration(pattern),
            section.GetValue<ValidationRule.ValidationCondition>(nameof(ValidationRule.Condition)),
            section.GetValue<string>(nameof(ValidationRule.Value))?? string.Empty
        );
    }
}
