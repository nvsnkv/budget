namespace NVs.Budget.Controllers.Web.Models;

public class CsvFileReadingConfiguration
{
    public string? CultureCode { get; init; }
    public string? EncodingName { get; init; }
    public string? DateTimeKind { get; init; }
    public Dictionary<string, string> Fields { get; init; } = new();
    public Dictionary<string, string> Attributes { get; init; } = new();
    public CsvValidationRuleExpression[] Validation { get; init; } = Array.Empty<CsvValidationRuleExpression>();

    public CsvFileReadingConfiguration()
    {
    }

    public CsvFileReadingConfiguration(
        string? cultureCode,
        string? encodingName,
        string? dateTimeKind,
        Dictionary<string, string> fields,
        Dictionary<string, string> attributes,
        CsvValidationRuleExpression[] validation)
    {
        CultureCode = cultureCode;
        EncodingName = encodingName;
        DateTimeKind = dateTimeKind;
        Fields = fields;
        Attributes = attributes;
        Validation = validation;
    }
}
