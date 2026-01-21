using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Domain.Extensions;
using NVs.Budget.Infrastructure.Files.CSV.Contracts;

namespace NVs.Budget.Infrastructure.Files.CSV.Persistence.Settings;

internal class BudgetSpecificSettingsRepository(SettingsContext context) : IReadingSettingsRepository
{
    private static readonly IReadOnlyDictionary<Regex, FileReadingSetting> Empty = new Dictionary<Regex, FileReadingSetting>().AsReadOnly();

    public async Task<IReadOnlyDictionary<Regex, FileReadingSetting>> GetReadingSettingsFor(TrackedBudget budget, CancellationToken ct) =>
        await context.CsvFileReadingSettings
            .Where(o => o.BudgetId == budget.Id && o.Deleted == false)
            .ToDictionaryAsync(
                x => new Regex(x.FileNamePattern),
                x => new FileReadingSetting(
                    CultureInfo.GetCultureInfo(x.Settings.CultureCode),
                    Encoding.GetEncoding(x.Settings.EncodingName),
                    x.Settings.DateTimeKind,
                    x.Settings.Fields.AsReadOnly(),
                    x.Settings.Attributes.AsReadOnly(),
                    x.Settings.Validation.AsReadOnly()
                ),
                ct);


    public async Task<Result> UpdateReadingSettingsFor(TrackedBudget budget, IReadOnlyDictionary<Regex, FileReadingSetting> settings, CancellationToken ct)
    {
        try
        {
            await using var transaction = await context.Database.BeginTransactionAsync(ct);

            var targets = await context.CsvFileReadingSettings.Where(x => x.BudgetId == budget.Id && x.Deleted == false).ToListAsync(ct);
            foreach (var target in targets)
            {
                target.Deleted = true;
                target.UpdatedAt = DateTime.UtcNow;
            }

            await context.CsvFileReadingSettings.AddRangeAsync(settings.Select(s => new StoredCsvFileReadingSettings()
                {
                    BudgetId = budget.Id,
                    FileNamePattern = s.Key.ToString(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Settings = new()
                    {
                        CultureCode = s.Value.Culture.Name,
                        EncodingName = s.Value.Encoding.WebName,
                        DateTimeKind = s.Value.DateTimeKind,
                        Attributes = new(s.Value.Attributes),
                        Fields = new(s.Value.Fields),
                        Validation = new(s.Value.Validation),
                    }
                }
            ), ct);

            await context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch (Exception ex)
        {
            return Result.Fail(new ExceptionalError(ex.WithBudgetId(budget)));
        }

        return Result.Ok();
    }
}
