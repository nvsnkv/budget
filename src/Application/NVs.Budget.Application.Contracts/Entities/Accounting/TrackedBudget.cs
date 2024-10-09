using NVs.Budget.Domain.Entities.Accounts;

namespace NVs.Budget.Application.Contracts.Entities.Accounting;

public class TrackedBudget(Guid id, string name, IEnumerable<Owner> owners)
    : Domain.Entities.Accounts.Budget(id, name, owners), ITrackableEntity<Guid>
{
    public string? Version { get; set; }
}
