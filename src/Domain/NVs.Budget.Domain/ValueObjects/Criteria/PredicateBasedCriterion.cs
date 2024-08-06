using NVs.Budget.Domain.Entities.Operations;

namespace NVs.Budget.Domain.ValueObjects.Criteria;

public class PredicateBasedCriterion : Criterion
{
    private readonly Func<Operation, bool> _predicate;

    public PredicateBasedCriterion(string description, Func<Operation, bool> predicate) : base(description)
    {
        _predicate = predicate;
    }

    public PredicateBasedCriterion(string description, Func<Operation, bool> predicate, IEnumerable<Criterion> subcriteria) : base(description, subcriteria)
    {
        _predicate = predicate;
    }

    public override bool Matched(Operation t)
    {
        return _predicate(t);
    }
}
