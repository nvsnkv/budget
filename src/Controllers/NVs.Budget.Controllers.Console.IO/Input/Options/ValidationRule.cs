namespace NVs.Budget.Controllers.Console.IO.Input.Options;

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
