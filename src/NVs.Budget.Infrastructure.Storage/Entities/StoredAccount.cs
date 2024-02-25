using System.ComponentModel.DataAnnotations;

namespace NVs.Budget.Infrastructure.Storage.Entities;

internal class StoredAccount(Guid id, string name, string bank) : DbRecord
{
    public static readonly StoredAccount Invalid = new(Guid.Empty, string.Empty, string.Empty);

    [Key]
    public Guid Id { get; private set; } = id;
    public string Name { get; set; } = name;
    public string Bank { get; set; } = bank;
    public string Version { get; set; } = string.Empty;

    public virtual IList<StoredOwner> Owners { get; init; } = new List<StoredOwner>();
    public virtual IList<StoredTransaction> Transactions { get; init; } = new List<StoredTransaction>();
}
