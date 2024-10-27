using System.Collections;
using System.Text;
using AutoMapper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Options;
using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Application.Contracts.Entities.Budgeting;
using NVs.Budget.Infrastructure.IO.Console.Models;
using NVs.Budget.Infrastructure.IO.Console.Options;

namespace NVs.Budget.Infrastructure.IO.Console.Output.Budgets;

internal class TrackedBudgetWriter(IOutputStreamProvider streams, IOptionsSnapshot<OutputOptions> options, IMapper mapper, CsvConfiguration config)
    : CsvObjectWriter<TrackedBudget, CsvBudget, CsvBudgetClassMap>(streams, options, mapper, config)
{
    public override Task Write(IEnumerable<TrackedBudget> collection, string streamName, CancellationToken ct) => DoWrite(collection, streamName, WriteDetails, ct);

    private async Task<bool> WriteDetails(StreamWriter writer, TrackedBudget budget, CancellationToken ct)
    {
        await writer.WriteLineAsync($"Tagging criteria: {budget.TaggingCriteria.Count}");
        await writer.WriteLineAsync($"Transfer criteria: {budget.TransferCriteria.Count}");
        await writer.WriteLineAsync($"Logbook criteria: {budget.LogbookCriteria.GetType().Name}");

        var subcriteria = CountSubcriteria(budget.LogbookCriteria).Aggregate("", (acc, count) => $"{acc}, {count}");
        await writer.WriteLineAsync($"Logbook subcriteria: {subcriteria}");

        return false;
    }

    private IEnumerable<int> CountSubcriteria(LogbookCriteria criteria)
    {
        if ((criteria.Subcriteria?.Count ?? 0) == 0)
        {
            yield break;
        }

        yield return criteria.Subcriteria!.Count;

        var childResults = criteria.Subcriteria.Select(subcriteria => CountSubcriteria(subcriteria).ToList()).ToList();

        int? counts;
        int i = 0;
        do
        {
            counts = null;
            foreach (var result in childResults)
            {
                if (result.Count <= i) {
                    continue;
                }

                counts += result[i];
            }

            if (counts.HasValue)
            {
                yield return counts.Value;

            }

        } while (counts.HasValue);
    }
}
