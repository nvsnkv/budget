using System.ComponentModel.DataAnnotations;

namespace NVs.Budget.Infrastructure.Persistence.EF.Entities;

internal class StoredTransfer(string comment) : DbRecord
{
    [Key]
    public Guid Id { get; init; }

    public StoredMoney Fee { get; set; } = StoredMoney.Zero;
    public string Comment { get; set; } = comment;

    public virtual StoredOperation Source { get; set; } = StoredOperation.Invalid;
    public virtual StoredOperation Sink { get; set; } = StoredOperation.Invalid;
}
