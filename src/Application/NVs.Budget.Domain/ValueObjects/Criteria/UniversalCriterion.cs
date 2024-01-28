using NVs.Budget.Domain.Entities.Transactions;

namespace NVs.Budget.Domain.ValueObjects.Criteria;

public class UniversalCriterion : PredicateBasedCriterion
{
    public UniversalCriterion(string description) : base(description, UniversalPredicate)
    {
    }

    public UniversalCriterion(string description, IEnumerable<Criterion> subcriteria) : base(description, UniversalPredicate, subcriteria)
    {
    }

    private static bool UniversalPredicate(Transaction _) => true;
}
