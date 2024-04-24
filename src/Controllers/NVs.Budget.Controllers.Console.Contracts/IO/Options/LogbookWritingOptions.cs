namespace NVs.Budget.Controllers.Console.Contracts.IO.Options;

public record LogbookWritingOptions(
    string Path,
    bool WriteCounts,
    bool WriteAmounts,
    bool WriteOperations,
    IEnumerable<NamedRange> Ranges);
