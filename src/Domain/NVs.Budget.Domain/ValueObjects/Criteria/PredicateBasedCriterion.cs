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

    protected bool Equals(PredicateBasedCriterion other)
    {
        return _predicate.Equals(other._predicate) && Description.Equals(other.Description);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((PredicateBasedCriterion)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_predicate.GetHashCode(), Description.GetHashCode());
    }
}
