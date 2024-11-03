using NVs.Budget.Domain.Entities.Operations;

namespace NVs.Budget.Domain.ValueObjects.Criteria;

public class SubcriteriaDrivenCriterion(string description, IEnumerable<Criterion> subcriteria) : Criterion(description, subcriteria)
{
    public SubcriteriaDrivenCriterion(string description) : this(description, Enumerable.Empty<Criterion>())
    {
    }

    public override bool Matched(Operation t)
    {
        return Subcriteria.Any(c => c.Matched(t));
    }
}
