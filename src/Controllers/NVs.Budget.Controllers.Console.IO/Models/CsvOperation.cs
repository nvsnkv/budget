namespace NVs.Budget.Controllers.Console.IO.Models;

internal class CsvOperation
{
    public DateTime Timestamp { get; init; }
    public string? Amount { get; init; }
    public string? Description { get; init; }
    public string? Tags { get; init; }
    public string? Attributes { get; init; }
    public string? Budget { get; init; }
    public string? Bank { get; init; }
}
