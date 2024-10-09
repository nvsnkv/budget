namespace NVs.Budget.Infrastructure.IO.Console.Input.Options;

internal record ValidationRule(
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
