namespace NVs.Budget.Infrastructure.IO.Console.Models;

internal class CsvTrackedOperation : CsvOperation
{
    public Guid Id { get; init; }
    public string? Version { get; init; }
    public Guid BudgetId { get; init; }
}
