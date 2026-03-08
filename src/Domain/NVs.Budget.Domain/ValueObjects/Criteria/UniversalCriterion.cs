using NVs.Budget.Domain.Entities.Operations;

namespace NVs.Budget.Domain.ValueObjects.Criteria;

public class UniversalCriterion : PredicateBasedCriterion
{
    public UniversalCriterion(string description) : base(description, UniversalPredicate)
    {
    }

    public UniversalCriterion(string description, Func<Operation, bool> precondition) : base(description, UniversalPredicate, precondition)
    {
    }

    public UniversalCriterion(string description, IEnumerable<Criterion> subcriteria) : base(description, UniversalPredicate, subcriteria)
    {
    }

    public UniversalCriterion(string description, IEnumerable<Criterion> subcriteria, Func<Operation, bool> precondition) : base(description, UniversalPredicate, subcriteria, precondition)
    {
    }

    private static bool UniversalPredicate(Operation _) => true;
}
