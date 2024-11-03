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

        var subcriteria = CountSubcriteria(budget.LogbookCriteria).Aggregate("Logbook criteria", (acc, count) => $"{acc}: {count}");
        await writer.WriteLineAsync(subcriteria);

        return false;
    }

    private IEnumerable<int> CountSubcriteria(LogbookCriteria criteria)
    {
        Queue<LogbookCriteria> current = new();
        Queue<LogbookCriteria> children = new();

        current.Enqueue(criteria);

        int value = 0;
        while (current.Count > 0)
        {
            var currentCriteria = current.Dequeue();
            if (currentCriteria.Subcriteria != null)
            {
                value += currentCriteria.Subcriteria.Count;

                foreach (var subcriteria in currentCriteria.Subcriteria)
                {
                    children.Enqueue(subcriteria);
                }
            }

            if (current.Count == 0)
            {
                if (value != 0)
                {
                    yield return value;
                }

                current = children;
                children = new();

                value = 0;
            }
        }
    }
}
