using System.ComponentModel.DataAnnotations;

namespace NVs.Budget.Infrastructure.Storage.Entities;

internal class StoredTransaction(Guid id, DateTime timestamp, string description)
{
    [Key]
    public Guid Id { get; private set; } = id;
    public DateTime Timestamp { get; set; } = timestamp;
    public StoredMoney? Amount { get; set; }
    public string Description { get; set; } = description;
    public string Version { get; set; } = string.Empty;

    public IList<StoredTag> Tags { get; init; } = new List<StoredTag>();
    public Dictionary<string, object> Attributes { get; init; } = new();

    public virtual StoredAccount Account { get; init; } = StoredAccount.Invalid;
}
