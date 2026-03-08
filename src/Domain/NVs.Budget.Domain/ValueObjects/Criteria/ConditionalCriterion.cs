using NVs.Budget.Domain.Entities.Operations;

namespace NVs.Budget.Domain.ValueObjects.Criteria;

public abstract class ConditionalCriterion : Criterion
{
    protected ConditionalCriterion(string description) : this(description, _ => true)
    {
    }

    protected ConditionalCriterion(string description, IEnumerable<Criterion> subcriteria) : this(description, subcriteria, _ => true)
    {
    }

    protected ConditionalCriterion(string description, Func<Operation, bool> precondition) : base(description)
    {
        Precondition = precondition;
    }

    protected ConditionalCriterion(string description, IEnumerable<Criterion> subcriteria, Func<Operation, bool> precondition) : base(description, subcriteria)
    {
        Precondition = precondition;
    }

    public Func<Operation, bool> Precondition { get; }

    public sealed override bool Matched(Operation t) => Precondition(t) && MatchedCore(t);

    protected abstract bool MatchedCore(Operation t);
}
