namespace NVs.Budget.Controllers.Web.Models;

public record CsvFileReadingConfiguration(
    string? CultureCode,
    string? EncodingName,
    string? DateTimeKind,
    Dictionary<string, string> Fields,
    Dictionary<string, string> Attributes,
    CsvValidationRuleExpression[] Validation);
