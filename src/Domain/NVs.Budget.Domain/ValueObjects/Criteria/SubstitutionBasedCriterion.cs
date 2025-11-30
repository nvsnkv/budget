using NVs.Budget.Domain.Entities.Operations;

namespace NVs.Budget.Domain.ValueObjects.Criteria;

public class SubstitutionBasedCriterion(string description, Func<Operation, string> substitution) : Criterion(description)
{
    private static readonly UniversalCriterion Dummy = new(string.Empty);

    private readonly SortedList<string, PredicateBasedCriterion> _subcriteria = new() { { Dummy.Description, Dummy } };

    public override IReadOnlyList<Criterion> Subcriteria => _subcriteria.Values.AsReadOnly();

    public override bool Matched(Operation t) => true;

    public override Criterion GetMatchedSubcriterion(Operation t)
    {
        var found = base.GetMatchedSubcriterion(t);
        if (found is not null && !ReferenceEquals(found, Dummy)) return found;

        if (_subcriteria.All(c => ReferenceEquals(c.Value, Dummy)))
        {
            _subcriteria.Clear();
        }

        var description = substitution(t);
        var criterion = new PredicateBasedCriterion(description, o => substitution(o) == description);
        _subcriteria.Add(description, criterion);
        return criterion;
    }
}
