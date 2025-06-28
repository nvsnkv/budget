using NVs.Budget.Infrastructure.Files.CSV.Contracts;

namespace NVs.Budget.Infrastructure.Files.CSV.Persistence.Settings;

internal class StoredSettings
{
    public static readonly StoredSettings Invalid = new();

    public string CultureCode { get; init; } = string.Empty;
    public string EncodingName { get; init; } = string.Empty;
    public Dictionary<string, string> Fields { get; init; } = new();
    public Dictionary<string, string> Attributes { get; init; } = new();
    public List<ValidationRule> Validation {get; init;} = new();
}
