using NMoneys;
using NVs.Budget.Domain.Entities.Accounts;
using NVs.Budget.Domain.Entities.Transactions;
using NVs.Budget.Domain.ValueObjects;

namespace NVs.Budget.Application.Services.Storage.Accounting;

public class TrackedAccount(Guid id, string name, string bank, IEnumerable<Owner> owners)
    : Account(id, name, bank, owners), ITrackableEntity<Guid>
{
    public string? Version { get; set; }
}

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
