namespace NVs.Budget.Infrastructure.IO.Console.Options;

public record ValidationRule(
    FieldConfiguration FieldConfiguration,
    ValidationRule.ValidationCondition Condition,
    string Value
)
{
    public enum ValidationCondition
    {
        Equals,
        NotEquals
    }
}
