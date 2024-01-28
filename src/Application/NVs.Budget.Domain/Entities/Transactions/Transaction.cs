using NMoneys;
using NVs.Budget.Domain.Entities.Accounts;
using NVs.Budget.Domain.ValueObjects;

namespace NVs.Budget.Domain.Entities.Transactions;

public class Transaction : EntityBase<Guid>
{
    private readonly List<Tag> _tags = new();

    public DateTime Timestamp { get; }
    public Money Amount { get; }
    public string Description { get; }
    public Account Account { get; }

    public IReadOnlyCollection<Tag> Tags => _tags.AsReadOnly();

    public IDictionary<string, object> Attributes { get; } = new Dictionary<string, object>();

    public Transaction(Guid id, DateTime timestamp, Money amount, string description, Account account, IEnumerable<Tag> tags, IReadOnlyDictionary<string, object>? attributes) : base(id)
    {
        Timestamp = timestamp;
        Amount = amount;
        Description = description;
        Account = account;

        if (attributes != null)
        {
            foreach (var attribute in attributes)
            {
                Attributes.Add(attribute);
            }
        }
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
