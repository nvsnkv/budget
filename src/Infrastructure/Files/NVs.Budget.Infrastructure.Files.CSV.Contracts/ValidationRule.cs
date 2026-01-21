namespace NVs.Budget.Infrastructure.Files.CSV.Contracts;

public record ValidationRule(
    string Pattern,
    ValidationRule.ValidationCondition Condition,
    string Value,
    string ErrorMessage
)
{
    public enum ValidationCondition
    {
        Equals,
        NotEquals,
    }
}
