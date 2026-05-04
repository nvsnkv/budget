using NMoneys;
using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Domain.Entities.Plans;
using NVs.Budget.Domain.ValueObjects.Criteria;

namespace NVs.Budget.Application.Contracts.Entities.Planning;

public record UnregisteredBudgetPlan(
    string Name,
    DateTime From,
    DateTime Till,
    string? CronExpression,
    Criterion Criterion,
    Currency Currency,
    IReadOnlyCollection<PlanExpectation> Expectations,
    Money? ExpectedAmount = null,
    string? Note = null,
    LogbookCriteria? LogbookCriteria = null);
