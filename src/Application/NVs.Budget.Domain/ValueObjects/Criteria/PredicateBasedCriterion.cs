using NVs.Budget.Domain.Entities.Transactions;

namespace NVs.Budget.Domain.ValueObjects.Criteria;

public class PredicateBasedCriterion : Criterion
{
    private readonly Predicate<Transaction> _predicate;

    public PredicateBasedCriterion(string description, Predicate<Transaction> predicate) : base(description)
    {
        _predicate = predicate;
    }

    public PredicateBasedCriterion(string description, Predicate<Transaction> predicate, IEnumerable<Criterion> subcriteria) : base(description, subcriteria)
    {
        _predicate = predicate;
    }

    public override bool Matched(Transaction t)
    {
        return _predicate(t);
    }
}
