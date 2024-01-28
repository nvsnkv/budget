using NVs.Budget.Domain.Entities.Transactions;

namespace NVs.Budget.Domain.ValueObjects.Criteria;

public abstract class Criterion
{
    private static readonly IReadOnlyList<Criterion> Empty = new List<Criterion>().AsReadOnly();

    protected Criterion(string description)
    {
        Description = description;
        Subcriteria = Empty;
    }

    protected Criterion(string description, IEnumerable<Criterion> subcriteria)
    {
        Description = description;
        Subcriteria = subcriteria.Distinct().ToList().AsReadOnly();
    }

    public string Description { get; }

    public IReadOnlyList<Criterion> Subcriteria { get; }

    public abstract bool Matched(Transaction t);

    public Criterion? GetMatchedSubcriterion(Transaction t)
    {
        return Subcriteria.FirstOrDefault(c => c.Matched(t));
    }
}
