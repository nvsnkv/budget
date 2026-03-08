using NVs.Budget.Domain.Entities.Operations;

namespace NVs.Budget.Domain.ValueObjects.Criteria;

public class SubcriteriaDrivenCriterion(string description, IEnumerable<Criterion> subcriteria, Func<Operation, bool> precondition)
    : ConditionalCriterion(description, subcriteria, precondition)
{
    protected override bool MatchedCore(Operation t) => Subcriteria.Any(c => c.Matched(t));
}
