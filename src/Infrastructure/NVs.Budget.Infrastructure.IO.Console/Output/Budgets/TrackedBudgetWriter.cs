using AutoMapper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Options;
using NVs.Budget.Application.Contracts.Entities.Budgeting;
using NVs.Budget.Infrastructure.IO.Console.Models;
using NVs.Budget.Infrastructure.IO.Console.Options;

namespace NVs.Budget.Infrastructure.IO.Console.Output.Budgets;

internal class TrackedBudgetWriter(IOutputStreamProvider streams, IOptionsSnapshot<OutputOptions> options, IMapper mapper, CsvConfiguration config)
    : CsvObjectWriter<TrackedBudget, CsvBudget, CsvBudgetClassMap>(streams, options, mapper, config)
{
    public override Task Write(IEnumerable<TrackedBudget> collection, CancellationToken ct) => DoWrite(collection, WriteDetails, ct);

    private async Task<bool> WriteDetails(TrackedBudget budget, CancellationToken ct)
    {
        var writer = await Streams.GetOutput(Options.Value.OutputStreamName);
        if (budget.TaggingCriteria.Any())
        {
            await writer.WriteLineAsync("| Tagging criteria");
            foreach (var rule in budget.TaggingCriteria)
            {
                await writer.WriteLineAsync($"| {rule.Tag} # {rule.Condition}");
            }
        }

        if (budget.TransferCriteria.Any())
        {
            await writer.WriteLineAsync("| Transfer criteria");
            foreach (var criterion in budget.TransferCriteria)
            {
                await writer.WriteLineAsync($"| {criterion.Accuracy}: {criterion.Comment}");
                await writer.WriteLineAsync($"| criterion: {criterion.Criterion}");
            }
        }

        return false;
    }
}
