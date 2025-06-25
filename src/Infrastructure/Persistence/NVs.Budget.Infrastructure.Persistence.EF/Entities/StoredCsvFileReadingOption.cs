using System.ComponentModel.DataAnnotations;

namespace NVs.Budget.Infrastructure.Persistence.EF.Entities;

internal class StoredCsvFileReadingOption : DbRecord
{
    public static StoredCsvFileReadingOption Invalid { get; } = new();

    [Key]
    public Guid Id { get; set; }
    public string CultureInfo { get; set; } = string.Empty;
    public DateTimeKind DateTimeKind { get; set; } = DateTimeKind.Local;
    public string FileNamePattern { get; set; } = string.Empty;
    public virtual StoredBudget Budget { get; set; } = StoredBudget.Invalid;

    public virtual IList<StoredFieldConfiguration> FieldConfigurations { get; set; } = new List<StoredFieldConfiguration>();
    public virtual IList<StoredFieldConfiguration> AttributesConfiguration { get; set; } = new List<StoredFieldConfiguration>();
    public virtual IList<StoredValidationRule> ValidationRules { get; set; } = new List<StoredValidationRule>();
}
