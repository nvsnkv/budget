using System.Globalization;
using System.Text.RegularExpressions;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Infrastructure.IO.Console.Options;
using NVs.Budget.Infrastructure.Persistence.EF.Context;
using NVs.Budget.Infrastructure.Persistence.EF.Entities;
using NVs.Budget.Infrastructure.Persistence.EF.Repositories.Results;

namespace NVs.Budget.Infrastructure.Persistence.EF.Repositories;

internal class BudgetSpecificSettingsRepository(BudgetContext context) : IBudgetSpecificSettingsRepository
{
    public async Task<CsvReadingOptions?> GetReadingOptionsFor(TrackedBudget budget, CancellationToken ct)
    {
        var options = await context.CsvFileReadingOptions
            .Include(o => o.FieldConfigurations)
            .Include(o => o.AttributesConfiguration)
            .Include(o => o.ValidationRules)
            .Where(o => o.Budget.Id == budget.Id && o.Deleted == false).ToListAsync(ct);

        if (options.Count == 0)
        {
            return null;
        }

        return new CsvReadingOptions(options.ToDictionary(o => new Regex(o.FileNamePattern), CreateFileReadingOption));
    }

    private CsvFileReadingOptions CreateFileReadingOption(StoredCsvFileReadingOption option)
    {
        var info = CultureInfo.GetCultureInfo(option.CultureInfo);
        var fieldConfigs = option.FieldConfigurations.ToDictionary(c => c.Field, o => new FieldConfiguration(o.Pattern));
        var attributesConfiguration = option.AttributesConfiguration.ToDictionary(c => c.Field, o => new FieldConfiguration(o.Pattern));
        var validationRules = option.ValidationRules.ToDictionary(
            v => v.RuleName, v => new ValidationRule(new(v.FieldConfiguration), v.Condition, v.Value)
        );

        return new CsvFileReadingOptions(fieldConfigs, info, option.DateTimeKind, attributesConfiguration, validationRules);
    }

    public  async Task<Result> UpdateReadingOptionsFor(TrackedBudget budget, CsvReadingOptions options, CancellationToken ct)
    {
        var storedBudget = await context.Budgets.Include(b => b.CsvReadingOptions).FirstOrDefaultAsync(b => b.Id == budget.Id, ct);
        if (storedBudget is null)
        {
            return Result.Fail(new BudgetDoesNotExistsError(budget));
        }

        storedBudget.CsvReadingOptions.Clear();

        foreach (var (key, opts) in options.Snapshot)
        {
            storedBudget.CsvReadingOptions.Add(new StoredCsvFileReadingOption()
            {
                FileNamePattern = key.ToString(),
                DateTimeKind = opts.DateTimeKind,
                CultureInfo = opts.CultureInfo.Name,

                FieldConfigurations = opts.Select((kv) => new StoredFieldConfiguration()
                {
                    Field = kv.Key,
                    Pattern = kv.Value.Pattern
                }).ToList(),

                AttributesConfiguration = opts.Attributes?.Select((kv) => new StoredFieldConfiguration()
                {
                    Field = kv.Key,
                    Pattern = kv.Value.Pattern
                }).ToList() ?? [],

                ValidationRules = opts.ValidationRules?.Select(kv => new StoredValidationRule()
                {
                    RuleName = kv.Key,
                    FieldConfiguration = kv.Value.FieldConfiguration.Pattern,
                    Condition = kv.Value.Condition,
                    Value = kv.Value.Value
                }).ToList() ?? []
            });
        }

        await context.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
