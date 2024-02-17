using NMoneys;
using NVs.Budget.Domain.Entities.Accounts;
using NVs.Budget.Domain.Entities.Transactions;
using NVs.Budget.Domain.ValueObjects;

namespace NVs.Budget.Application.Entities.Accounting;

public class TrackedTransaction(
    Guid id,
    DateTime timestamp,
    Money amount,
    string description,
    Account account,
    IEnumerable<Tag> tags,
    IReadOnlyDictionary<string, object>? attributes)
    : Transaction(id, timestamp, amount, description, account, tags, attributes), ITrackableEntity<Guid>
{
    public string? Version { get; set; }
}
