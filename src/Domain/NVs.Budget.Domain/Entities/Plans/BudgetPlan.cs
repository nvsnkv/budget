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
        if (!TryGetCriterionPath(Criterion, criterion, out var criterionPath))
        {
            throw new InvalidOperationException("Criterion is not part of this plan's criterion tree.");
        }

        var windowExpectations = _expectations.Where(e => e.From == from && e.Till == till).ToList();

        var amounts = windowExpectations
            .Where(e => !string.IsNullOrEmpty(e.SubcriterionPath))
            .Where(e => ExpectationMatchesCriterionPath(criterionPath, e.SubcriterionPath!, criterion))
            .Select(e => e.ExpectedAmount)
            .ToList();

        if (ReferenceEquals(criterion, Criterion))
        {
            amounts.AddRange(windowExpectations
                .Where(e => string.IsNullOrEmpty(e.SubcriterionPath))
                .Select(e => e.ExpectedAmount));
        }

        return Sum(amounts);
    }

    /// <summary>
    /// Builds "Description/Child/Leaf" paths matching logbook criteria traversal (root description included).
    /// </summary>
    private static bool TryGetCriterionPath(Criterion root, Criterion target, out string path)
    {
        if (ReferenceEquals(root, target))
        {
            path = root.Description;
            return true;
        }

        foreach (var sub in root.Subcriteria)
        {
            if (!TryGetCriterionPath(sub, target, out var tail)) continue;

            path = string.IsNullOrEmpty(tail)
                ? $"{root.Description}/{sub.Description}"
                : $"{root.Description}/{tail}";
            return true;
        }

        path = string.Empty;
        return false;
    }

    /// <summary>
    /// Hierarchical paths use prefix roll-up (node path + all descendant paths).
    /// Legacy values without '/' keep the previous "any matching description in this subtree" behaviour.
    /// </summary>
    private static bool ExpectationMatchesCriterionPath(
        string criterionPath,
        string expectationPath,
        Criterion criterion)
    {
        var pathComparison = StringComparison.OrdinalIgnoreCase;
        if (!expectationPath.Contains('/'))
        {
            var legacyNames = GetCriterionDescriptionsInSubtree(criterion).ToHashSet(StringComparer.OrdinalIgnoreCase);
            return legacyNames.Contains(expectationPath);
        }

        if (string.Equals(criterionPath, expectationPath, pathComparison))
        {
            return true;
        }

        var prefix = criterionPath + "/";
        return expectationPath.StartsWith(prefix, pathComparison);
    }

    private static IEnumerable<string> GetCriterionDescriptionsInSubtree(Criterion criterion)
    {
        yield return criterion.Description;
        foreach (var childName in criterion.Subcriteria.SelectMany(GetCriterionDescriptionsInSubtree))
        {
            yield return childName;
        }
    }

    private Money Sum(IReadOnlyCollection<Money> amounts)
    {
        return amounts.Count == 0
            ? new Money(0, Currency)
            : amounts.Aggregate((left, right) => left + right);
    }

}
