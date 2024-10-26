using NMoneys;
using NVs.Budget.Domain.ValueObjects;

namespace NVs.Budget.Domain.Entities.Operations;

public class Operation : EntityBase<Guid>
{
    private readonly List<Tag> _tags;

    public DateTime Timestamp { get; }
    public Money Amount { get; }
    public string Description { get; }
    public Accounts.Budget Budget { get; }

    public IReadOnlyCollection<Tag> Tags => _tags.AsReadOnly();

    public IDictionary<string, object> Attributes { get; } = new AttributesDictionary(new Dictionary<string, object>());

    public Operation(Guid id, DateTime timestamp, Money amount, string description, Accounts.Budget budget, IEnumerable<Tag> tags, IReadOnlyDictionary<string, object>? attributes) : base(id)
    {
        Timestamp = timestamp;
        Amount = amount;
        Description = description;
        Budget = budget;

        if (attributes != null)
        {
            foreach (var attribute in attributes)
            {
                Attributes.Add(attribute);
            }
        }

        _tags = tags.Distinct().ToList();
    }

    public void Tag(Tag value)
    {
        if (!_tags.Contains(value))
        {
            _tags.Add(value);
        }
    }

    public void Untag(Tag value) => _tags.Remove(value);
}
