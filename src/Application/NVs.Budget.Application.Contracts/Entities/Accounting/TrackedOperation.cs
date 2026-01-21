using NMoneys;
using NVs.Budget.Domain.Entities.Operations;
using NVs.Budget.Domain.ValueObjects;

namespace NVs.Budget.Application.Contracts.Entities.Accounting;

public class  TrackedOperation(
    Guid id,
    DateTime timestamp,
    Money amount,
    string description,
    Domain.Entities.Budgets.Budget budget,
    IEnumerable<Tag> tags,
    IReadOnlyDictionary<string, object>? attributes)
    : Operation(id, timestamp, amount, description, budget, tags, attributes), ITrackableEntity<Guid>
{
    public string? Version { get; set; }
    public bool IsRegistered  => !string.IsNullOrEmpty(Version);
}
