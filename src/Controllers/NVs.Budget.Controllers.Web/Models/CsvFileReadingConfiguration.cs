namespace NVs.Budget.Controllers.Web.Models;

public class CsvFileReadingConfiguration : Dictionary<string, string> {
    public string CultureCode { get; init; }
    public DateTimeKind DateTimeKind { get; init; }
    public IReadOnlyDictionary<string, string>? Attributes {get; init;}
    public IReadOnlyDictionary<string, string>? ValidationRules {get; init;}

    public class ValidationRule {
        public string FieldConfiguration {get; init;}
        public string Value {get; init;}
        public ValidationCondition Condition {get; init;}
    }

    public enum ValidationCondition {
        Equals,
        NotEquals
    }
}




