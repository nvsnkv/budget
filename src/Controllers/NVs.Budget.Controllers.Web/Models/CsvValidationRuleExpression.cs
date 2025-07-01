namespace NVs.Budget.Controllers.Web.Models;

public class CsvValidationRuleExpression
{
    public string Pattern { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public string ErrorMessage { get; init; } = string.Empty;
    public CsvValidationCondition Condition { get; init; } = CsvValidationCondition.Equals;

    public CsvValidationRuleExpression()
    {
    }

    public CsvValidationRuleExpression(
        string pattern,
        string value,
        string errorMessage,
        CsvValidationCondition condition)
    {
        Pattern = pattern;
        Value = value;
        ErrorMessage = errorMessage;
        Condition = condition;
    }
}
