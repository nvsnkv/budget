using System.Text.RegularExpressions;
using FluentResults;
using NVs.Budget.Application.Contracts.Entities.Budgeting;

namespace NVs.Budget.Infrastructure.Files.CSV.Contracts;

public interface IReadingSettingsRepository
{
    Task<IReadOnlyDictionary<Regex, FileReadingSetting>> GetReadingSettingsFor(TrackedBudget budget, CancellationToken ct);

    Task<Result> UpdateReadingSettingsFor(TrackedBudget budget, IReadOnlyDictionary<Regex, FileReadingSetting> settings, CancellationToken ct);
}
