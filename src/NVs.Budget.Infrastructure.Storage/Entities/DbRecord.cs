namespace NVs.Budget.Infrastructure.Storage.Entities;

internal class DbRecord
{
    public DateTime Created { get; init; } = DateTime.UtcNow;
    public DateTime? Updated { get; set; }
    public bool Deleted { get; set; }
}
