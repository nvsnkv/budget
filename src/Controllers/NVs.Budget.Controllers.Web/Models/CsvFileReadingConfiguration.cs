namespace NVs.Budget.Controllers.Web.Models;

public class CsvFileReadingConfiguration : Dictionary<string, string> {
    public string CultureCode { get; init; } = string.Empty;
    public DateTimeKind DateTimeKind { get; init; }
    public IReadOnlyDictionary<string, string>? Attributes {get; init;}
    public IReadOnlyDictionary<string, string>? ValidationRules {get; init;}

    public class ValidationRule {
        public string FieldConfiguration {get; init;} = string.Empty;
        public string Value {get; init;} = string.Empty;
        public ValidationCondition Condition {get; init;} = ValidationCondition.Equals;
    }

    public enum ValidationCondition {
        Equals,
        NotEquals
    }
}




