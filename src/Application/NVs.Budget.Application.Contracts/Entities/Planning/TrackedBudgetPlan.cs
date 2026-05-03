using NMoneys;
using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Application.Contracts.Entities;
using NVs.Budget.Domain.Entities.Plans;
using NVs.Budget.Domain.ValueObjects.Criteria;

namespace NVs.Budget.Application.Contracts.Entities.Planning;

public class TrackedBudgetPlan(
    Guid id,
    Guid budgetId,
    string name,
    DateTime from,
    DateTime till,
    string? cronExpression,
    Criterion criterion,
    Currency currency,
    IEnumerable<PlanExpectation>? expectations = null,
    Money? expectedAmount = null,
    string? note = null,
    LogbookCriteria? logbookCriteria = null)
    : BudgetPlan(id, name, from, till, cronExpression, criterion, currency, expectations, expectedAmount, note),
        ITrackableEntity<Guid>
{
    public Guid BudgetId { get; } = budgetId;

    public LogbookCriteria? LogbookCriteria { get; } = logbookCriteria;

    public string? Version { get; set; }
}
