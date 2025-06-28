using System.Globalization;
using System.Text;

namespace NVs.Budget.Infrastructure.Files.CSV.Contracts;

public record FileReadingSetting(
    CultureInfo Culture,
    Encoding Encoding,
    IReadOnlyDictionary<string, string> Fields,
    IReadOnlyDictionary<string, string> Attributes,
    IReadOnlyCollection<ValidationRule> Validation);
