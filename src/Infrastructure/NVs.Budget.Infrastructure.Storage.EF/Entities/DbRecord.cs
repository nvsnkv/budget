namespace NVs.Budget.Infrastructure.Storage.Entities;

internal class DbRecord
{
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public bool Deleted { get; set; }
}
