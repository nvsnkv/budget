namespace NVs.Budget.Controllers.Console.IO.Models;

internal class CsvTrackedOperation : CsvOperation
{
    public Guid Id { get; init; }
    public string? Version { get; init; }
    public Guid AccountId { get; init; }
}
