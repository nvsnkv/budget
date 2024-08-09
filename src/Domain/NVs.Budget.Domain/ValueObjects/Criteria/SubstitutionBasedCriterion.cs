using NVs.Budget.Domain.Entities.Operations;

namespace NVs.Budget.Domain.ValueObjects.Criteria;

public class SubstitutionBasedCriterion : Criterion
{
    private static readonly UniversalCriterion Dummy = new(string.Empty);

    private readonly List<PredicateBasedCriterion> _subcriteria = [Dummy];
    private readonly Func<Operation, string> _substitution;

    public SubstitutionBasedCriterion(string description, Func<Operation, string> substitution) : base(description)
    {
        _substitution = substitution;
    }

    public override IReadOnlyList<Criterion> Subcriteria => _subcriteria.AsReadOnly();

    public override bool Matched(Operation t) => true;

    public override Criterion? GetMatchedSubcriterion(Operation t)
    {
        var found = base.GetMatchedSubcriterion(t);
        if (found is not null && !ReferenceEquals(found, Dummy)) return found;

        if (_subcriteria.All(c => ReferenceEquals(c, Dummy)))
        {
            _subcriteria.Clear();
        }

        var description = _substitution(t);
        var criterion = new PredicateBasedCriterion(description, o => _substitution(o) == description);
        _subcriteria.Add(criterion);
        return criterion;
    }
}
