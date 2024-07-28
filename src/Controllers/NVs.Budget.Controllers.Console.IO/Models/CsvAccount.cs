using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using NVs.Budget.Domain.Entities.Accounts;

namespace NVs.Budget.Controllers.Console.IO.Output.Accounts;

internal class CsvAccount
{
    public Guid Id { get; init; }

    public string? Name { get; init; }

    public string? Bank { get; init; }

    public string? Owners { get; init; }
}
