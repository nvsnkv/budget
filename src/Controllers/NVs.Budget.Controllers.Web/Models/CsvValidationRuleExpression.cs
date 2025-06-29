namespace NVs.Budget.Controllers.Web.Models;

public record CsvValidationRuleExpression(
    string Pattern,
    string Value,
    string ErrorMessage,
    CsvValidationCondition Condition);
