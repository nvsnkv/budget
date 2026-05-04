using NMoneys;

namespace NVs.Budget.Domain.Aggregates.Plans;

public class Variance
{
    public Variance(
        string description,
        PlanRange range,
        Money expected,
        Money actual,
        IReadOnlyCollection<Variance>? children = null)
    {
        Description = description;
        Range = range;
        Expected = expected;
        Actual = actual;
        Difference = actual - expected;
        Children = children ?? [];
    }

    public string Description { get; }

    public PlanRange Range { get; }

    public Money Expected { get; }

    public Money Actual { get; }

    public Money Difference { get; }

    public IReadOnlyCollection<Variance> Children { get; }

    public bool HasPlan => Expected.Amount != 0;

    public bool HasActual => Actual.Amount != 0;

    public bool IsPlannedOnly => HasPlan && !HasActual;

    public int ActualComparison => Actual.Amount.CompareTo(Expected.Amount);
}
