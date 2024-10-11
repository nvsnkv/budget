using System.Globalization;

namespace NVs.Budget.Infrastructure.IO.Console.Options;

public class SpecificCsvFileReadingOptions(
    string fileName,
    CsvFileReadingOptions options
) : CsvFileReadingOptions(options.ToDictionary(), options.CultureInfo, options.DateTimeKind, options.Attributes, options.ValidationRules)
{
    public string FileName { get; } = fileName;
}
