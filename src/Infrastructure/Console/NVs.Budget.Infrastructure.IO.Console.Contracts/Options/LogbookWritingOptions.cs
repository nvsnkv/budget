namespace NVs.Budget.Infrastructure.IO.Console.Options;

public record LogbookWritingOptions(
    string Path,
    bool WriteCounts,
    bool WriteOperations,
    IEnumerable<NamedRange> Ranges);
