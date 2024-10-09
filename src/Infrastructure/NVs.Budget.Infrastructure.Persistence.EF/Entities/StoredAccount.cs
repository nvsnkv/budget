using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using NVs.Budget.Application.Contracts.Entities;

namespace NVs.Budget.Infrastructure.Persistence.EF.Entities;

[DebuggerDisplay("{GetType().Name} {Id}")]
[SuppressMessage("ReSharper", "ClassWithVirtualMembersNeverInherited.Global", Justification = "Virtual nav props are required to perform lazy load")]
internal class StoredAccount(Guid id, string name) : DbRecord, ITrackableEntity<Guid>
{
    public static readonly StoredAccount Invalid = new(Guid.Empty, string.Empty);

    [Key]
    public Guid Id { get; [UsedImplicitly] private set; } = id;
    public string Name { get; set; } = name;
    public string? Version { get; set; } = string.Empty;

    public virtual IList<StoredOwner> Owners { get; init; } = new List<StoredOwner>();
    public virtual IList<StoredOperation> Operations { get; init; } = new List<StoredOperation>();
}
