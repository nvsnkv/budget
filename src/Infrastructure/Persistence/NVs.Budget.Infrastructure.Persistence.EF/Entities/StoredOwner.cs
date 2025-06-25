using System.ComponentModel.DataAnnotations;
using NVs.Budget.Application.Contracts.Entities;

namespace NVs.Budget.Infrastructure.Persistence.EF.Entities;

internal class StoredOwner(Guid id, string name) : DbRecord, ITrackableEntity<Guid>
{
    public static readonly StoredOwner Invalid = new(Guid.Empty, string.Empty);

    [Key]
    public Guid Id { get; private set; } = id;

    public string Name { get; set; } = name;
    public string UserId { get; set; } = string.Empty;
    public string? Version { get; set; }

    public virtual IList<StoredBudget> Accounts { get; init; } = new List<StoredBudget>();
}
