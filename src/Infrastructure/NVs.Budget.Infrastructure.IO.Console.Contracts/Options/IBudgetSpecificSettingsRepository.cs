using FluentResults;
using NVs.Budget.Application.Contracts.Entities.Accounting;

namespace NVs.Budget.Infrastructure.IO.Console.Options;

public interface IBudgetSpecificSettingsRepository
{
    Task<CsvReadingOptions> GetReadingOptionsFor(TrackedBudget budget, CancellationToken ct);
    Task<Result> UpdateReadingOptionsFor(TrackedBudget budget, CsvReadingOptions options, CancellationToken ct);
}
