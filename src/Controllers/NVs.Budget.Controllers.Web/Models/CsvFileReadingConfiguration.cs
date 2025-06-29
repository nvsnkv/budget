namespace NVs.Budget.Controllers.Web.Models;

public record CsvFileReadingConfiguration(
    string? CultureCode,
    string? EncodingName,
    Dictionary<string, string> Fields,
    Dictionary<string, string> Attributes,
    CsvValidationRuleExpression[] Validation);
