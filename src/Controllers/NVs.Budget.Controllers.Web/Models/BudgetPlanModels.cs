namespace NVs.Budget.Controllers.Web.Models;

public record PlanExpectationResponse(
    Guid Id,
    MoneyResponse ExpectedAmount,
    DateTime From,
    DateTime Till,
    string? SubcriterionPath,
    string? SubcriterionName,
    string? Note);

public record BudgetPlanResponse(
    Guid Id,
    Guid BudgetId,
    string Name,
    string Version,
    DateTime From,
    DateTime Till,
    string? CronExpression,
    LogbookCriteriaResponse LogbookCriteria,
    string CurrencyCode,
    MoneyResponse ExpectedAmount,
    string? Note,
    IReadOnlyCollection<PlanExpectationResponse> Expectations);

public record UpsertPlanExpectationRequest(
    Guid? Id,
    MoneyResponse ExpectedAmount,
    DateTime From,
    DateTime Till,
    string? SubcriterionPath,
    string? SubcriterionName,
    string? Note);

public record UpsertBudgetPlanRequest(
    string Name,
    string? Version,
    DateTime From,
    DateTime Till,
    string? CronExpression,
    LogbookCriteriaResponse LogbookCriteria,
    string CurrencyCode,
    MoneyResponse? ExpectedAmount,
    string? Note,
    IReadOnlyCollection<UpsertPlanExpectationRequest>? Expectations);

public record CopyBudgetPlanRequest(string? Name);

public record VarianceResponse(
    string Description,
    NamedRangeResponse Range,
    MoneyResponse Expected,
    MoneyResponse Actual,
    MoneyResponse Difference,
    bool HasPlan,
    bool HasActual,
    bool IsPlannedOnly,
    int ActualComparison,
    IReadOnlyCollection<VarianceResponse> Children);

public record VarianceReportResponse(
    BudgetPlanResponse Plan,
    IReadOnlyCollection<VarianceResponse> Variances);
