namespace NVs.Budget.Infrastructure.Persistence.EF.Entities;

internal class DbRecord
{
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public bool Deleted { get; set; }
}
