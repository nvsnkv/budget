using NMoneys;
using NVs.Budget.Domain.ValueObjects.Criteria;

namespace NVs.Budget.Domain.Entities.Plans;

public class BudgetPlan : PlanExpectation
{
    private readonly List<PlanExpectation> _expectations;

    public BudgetPlan(
        Guid id,
        string name,
        DateTime from,
        DateTime till,
        string? cronExpression,
        Criterion criterion,
        Currency currency,
        IEnumerable<PlanExpectation>? expectations = null,
        Money? expectedAmount = null,
        string? note = null)
        : base(id, expectedAmount ?? new Money(0, currency), from, till, null, note)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Plan name is required.", nameof(name));
        }

        Name = name.Trim();
        CronExpression = string.IsNullOrWhiteSpace(cronExpression) ? null : cronExpression.Trim();
        Criterion = criterion;
        Currency = currency;
        _expectations = [];

        foreach (var expectation in expectations ?? [])
        {
            AddExpectation(expectation);
        }
    }

    public string Name { get; }

    public string? CronExpression { get; }

    public Criterion Criterion { get; }

    public Currency Currency { get; }

    public IReadOnlyCollection<PlanExpectation> Expectations => _expectations.AsReadOnly();

    public void AddExpectation(PlanExpectation expectation)
    {
        if (expectation.From < From || expectation.Till > Till)
        {
            throw new ArgumentException("Expectation range must be inside the plan range.", nameof(expectation));
        }

        if (expectation.ExpectedAmount.GetCurrency() != Currency)
        {
            throw new ArgumentException("Expectation currency must match the plan currency.", nameof(expectation));
        }

        _expectations.Add(expectation);
    }

    public Money GetExpectedAmount(Criterion criterion, DateTime from, DateTime till)
    {
        var matchingNames = GetCriterionNames(criterion).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var expectations = _expectations
            .Where(e => e.From == from && e.Till == till)
            .Where(e => e.SubcriterionName is not null && matchingNames.Contains(e.SubcriterionName))
            .Select(e => e.ExpectedAmount)
            .ToList();

        if (ReferenceEquals(criterion, Criterion))
        {
            expectations.AddRange(_expectations
                .Where(e => e.From == from && e.Till == till && e.SubcriterionName is null)
                .Select(e => e.ExpectedAmount));
        }

        return Sum(expectations);
    }

    private Money Sum(IReadOnlyCollection<Money> amounts)
    {
        return amounts.Count == 0
            ? new Money(0, Currency)
            : amounts.Aggregate((left, right) => left + right);
    }

    private static IEnumerable<string> GetCriterionNames(Criterion criterion)
    {
        yield return criterion.Description;

        foreach (var childName in criterion.Subcriteria.SelectMany(GetCriterionNames))
        {
            yield return childName;
        }
    }
}
