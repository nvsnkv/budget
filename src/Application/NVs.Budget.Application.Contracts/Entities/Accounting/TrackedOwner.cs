using NVs.Budget.Domain.Entities.Budgets;

namespace NVs.Budget.Application.Contracts.Entities.Budgeting;

public class TrackedOwner(Guid id, string name) : Owner(id, name), ITrackableEntity<Guid>
{
    public string? Version { get; set; }
}
