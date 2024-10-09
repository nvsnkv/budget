namespace NVs.Budget.Infrastructure.IO.Console.Models;

internal class CsvAccount
{
    public Guid Id { get; init; }

    public string? Name { get; init; }

    public string? Bank { get; init; }

    public string? Owners { get; init; }
}
