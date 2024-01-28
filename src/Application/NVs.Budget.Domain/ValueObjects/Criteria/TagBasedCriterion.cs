using NVs.Budget.Domain.Entities.Transactions;

namespace NVs.Budget.Domain.ValueObjects.Criteria;

public class TagBasedCriterion : Criterion
{
    private readonly List<Tag> _tags = new();

    public TagBasedCriterion(string description, IEnumerable<Tag> tags, TagBasedCriterionType type) : base(description)
    {
        Type = type;
        _tags.AddRange(tags.Distinct());
        if (!_tags.Any()) throw new ArgumentException("No tags provided!", nameof(tags));
    }

    public TagBasedCriterion(string description, IEnumerable<Tag> tags, TagBasedCriterionType type, IEnumerable<Criterion> subcriteria) : base(description, subcriteria)
    {
        Type = type;
        _tags.AddRange(tags.Distinct());
        if (!_tags.Any()) throw new ArgumentException("No tags provided!", nameof(tags));
    }

    public TagBasedCriterionType Type { get; }

    public override bool Matched(Transaction t)
    {
        switch (Type)
        {
            case TagBasedCriterionType.Including:
                return _tags.All(tag => t.Tags.Contains(tag));

            case TagBasedCriterionType.Excluding:
                return !_tags.Any(tag => t.Tags.Contains(tag));

            default:
                throw new ArgumentOutOfRangeException(nameof(Type));
        }
    }
}
