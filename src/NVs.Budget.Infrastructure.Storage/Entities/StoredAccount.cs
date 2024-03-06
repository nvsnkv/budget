using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using NVs.Budget.Application.Entities;

namespace NVs.Budget.Infrastructure.Storage.Entities;

[DebuggerDisplay("{GetType().Name} {Id}")]
[SuppressMessage("ReSharper", "ClassWithVirtualMembersNeverInherited.Global", Justification = "Virtual nav props are required to perform lazy load")]
internal class StoredAccount(Guid id, string name, string bank) : DbRecord, ITrackableEntity<Guid>
{
    public static readonly StoredAccount Invalid = new(Guid.Empty, string.Empty, string.Empty);

    [Key]
    public Guid Id { get; [UsedImplicitly] private set; } = id;
    public string Name { get; set; } = name;
    public string Bank { get; set; } = bank;
    //TODO rebuild migration to allow null
    public string Version { get; set; } = string.Empty;

    public virtual IList<StoredOwner> Owners { get; init; } = new List<StoredOwner>();
    public virtual IList<StoredTransaction> Transactions { get; init; } = new List<StoredTransaction>();
}
