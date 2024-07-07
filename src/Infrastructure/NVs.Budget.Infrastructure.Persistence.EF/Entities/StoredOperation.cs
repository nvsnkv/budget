using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using NVs.Budget.Application.Contracts.Entities;

namespace NVs.Budget.Infrastructure.Persistence.EF.Entities;

[SuppressMessage("ReSharper", "ClassWithVirtualMembersNeverInherited.Global", Justification = "Virtual nav props are required to perform lazy load")]
internal class StoredOperation(Guid id, DateTime timestamp, string description) : DbRecord, ITrackableEntity<Guid>
{
    [Key]
    public Guid Id { get; [UsedImplicitly] private set; } = id;
    public DateTime Timestamp { get; set; } = timestamp;
    public StoredMoney Amount { get; set; } = StoredMoney.Zero;
    public string Description { get; set; } = description;
    public string? Version { get; set; } = string.Empty;

    public IList<StoredTag> Tags { get; init; } = new List<StoredTag>();
    public Dictionary<string, object> Attributes { get; set; } = new();

    public virtual StoredAccount Account { get; set; } = StoredAccount.Invalid;
    public virtual StoredTransfer? SourceTransfer { get; set; }
    public virtual StoredTransfer? SinkTransfer { get; set; }

    public static readonly StoredOperation Invalid = new(Guid.Empty, DateTime.MinValue, string.Empty);
}
