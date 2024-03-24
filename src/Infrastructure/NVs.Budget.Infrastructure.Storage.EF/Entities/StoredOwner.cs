using System.ComponentModel.DataAnnotations;

namespace NVs.Budget.Infrastructure.Storage.Entities;

internal class StoredOwner(Guid id, string name) : DbRecord
{
    public static readonly StoredOwner Invalid = new(Guid.Empty, string.Empty);

    [Key]
    public Guid Id { get; private set; } = id;
    public string Name { get; set; } = name;

    public virtual IList<StoredAccount> Accounts { get; init; }= new List<StoredAccount>();
}
