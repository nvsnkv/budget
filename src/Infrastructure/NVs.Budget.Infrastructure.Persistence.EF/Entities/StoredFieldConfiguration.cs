namespace NVs.Budget.Infrastructure.Persistence.EF.Entities;

internal class StoredFieldConfiguration
{
    public string Field { get; set; } = string.Empty;
    public string Pattern { get; set; } = string.Empty;

    public virtual StoredCsvFileReadingOption FileReadingOption { get; set; } = StoredCsvFileReadingOption.Invalid;
}