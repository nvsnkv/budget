using System.ComponentModel.DataAnnotations;

namespace NVs.Budget.Infrastructure.Files.CSV.Persistence.Settings;

internal class StoredCsvFileReadingSettings
{
    [Key] public Guid Id { get; init; }
    public Guid BudgetId { get; init; }
    public string FileNamePattern { get; init; } = string.Empty;
    public StoredSettings Settings { get; set; } = StoredSettings.Invalid;
    public bool Deleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
