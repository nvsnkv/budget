
using NVs.Budget.Infrastructure.Files.CSV.Contracts;

namespace NVs.Budget.Infrastructure.Persistence.EF.Entities;

internal class StoredValidationRule
{
    public string RuleName { get; set; } = string.Empty;
    public string FieldConfiguration { get; set; } = string.Empty;
    public ValidationRule.ValidationCondition Condition { get; set; }
    public string Value { get; set; } = string.Empty;

    public virtual StoredCsvFileReadingOption FileReadingOption { get; set; } = StoredCsvFileReadingOption.Invalid;
}
