using NMoneys;
using NVs.Budget.Domain.Entities.Plans;
using NVs.Budget.Domain.ValueObjects.Criteria;

namespace NVs.Budget.Domain.Aggregates.Plans;

public class VarianceReport
{
    private readonly List<Variance> _variances;

    public VarianceReport(BudgetPlan plan, CriteriaBasedLogbook logbook, IEnumerable<PlanRange> ranges)
    {
        Plan = plan;
        _variances = ranges
            .Select(range => BuildVariance(plan, plan.Criterion, (CriteriaBasedLogbook)logbook[range.From, range.Till], range))
            .ToList();
    }

    public BudgetPlan Plan { get; }

    public IReadOnlyCollection<Variance> Variances => _variances.AsReadOnly();

    private static Variance BuildVariance(
        BudgetPlan plan,
        Criterion criterion,
        CriteriaBasedLogbook logbook,
        PlanRange range)
    {
        var children = criterion.Subcriteria
            .Select(childCriterion =>
            {
                var childLogbook = logbook.Children.TryGetValue(childCriterion, out var value)
                    ? value
                    : new CriteriaBasedLogbook(childCriterion);

                return BuildVariance(plan, childCriterion, childLogbook, range);
            })
            .ToList();

        var expected = plan.GetExpectedAmount(criterion, range.From, range.Till);
        var actual = NormalizeActual(logbook, plan.Currency);

        return new Variance(criterion.Description, range, expected, actual, children);
    }

    private static Money NormalizeActual(CriteriaBasedLogbook logbook, Currency currency)
    {
        return logbook.IsEmpty ? new Money(0, currency) : logbook.Sum;
    }
}
