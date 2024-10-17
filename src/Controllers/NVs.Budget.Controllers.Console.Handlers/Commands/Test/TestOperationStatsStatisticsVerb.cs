using System.Text;
using CommandLine;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Options;
using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Controllers.Console.Contracts.Commands;
using NVs.Budget.Infrastructure.IO.Console.Input;
using NVs.Budget.Infrastructure.IO.Console.Options;
using NVs.Budget.Infrastructure.IO.Console.Output;

namespace NVs.Budget.Controllers.Console.Handlers.Commands.Test;

[Verb("ops-stats", HelpText = "Validates ruleset")]
internal class TestOperationStatsStatisticsVerb : AbstractVerb
{
    [Option('r', "ruleset", Required = true, HelpText = "Path to ruleset used for aggregation")]
    public string Ruleset { get; set; } = "";
}


internal class TestOperationStatsStatisticsVerbHandler(
    ILogbookCriteriaReader reader,
    IInputStreamProvider inputStreamProvider,
    IOutputStreamProvider outputStreamProvider,
    IOptionsSnapshot<OutputOptions> outputOptions,
    IResultWriter<Result> writer
) : IRequestHandler<TestOperationStatsStatisticsVerb, ExitCode>
{
    public async Task<ExitCode> Handle(TestOperationStatsStatisticsVerb request, CancellationToken cancellationToken)
    {
        var stream = await inputStreamProvider.GetInput(request.Ruleset);
        if (!stream.IsSuccess)
        {
            await writer.Write(stream.ToResult(), cancellationToken);
            return stream.ToExitCode();

        }

        var criteria = await reader.ReadFrom(stream.Value, cancellationToken);
        if (!criteria.IsSuccess)
        {
            await writer.Write(criteria.ToResult(), cancellationToken);
            return criteria.ToExitCode();
        }

        var output = await outputStreamProvider.GetOutput(outputOptions.Value.OutputStreamName);
        await WriteCriterion(output, criteria.Value, "");
        await output.FlushAsync(cancellationToken);

        return ExitCode.Success;
    }

    private async Task WriteCriterion(StreamWriter output, LogbookCriteria criterion, string padding)
    {
        await output.WriteLineAsync($"{padding}Description: {criterion.Description}");
        if (criterion.Tags != null)
        {
            await output.WriteLineAsync($"{padding}Tags: [" + criterion.Tags.Aggregate(new StringBuilder(), (a, v) => a.Append($"{v.Value} ")) + "]");
            await output.WriteLineAsync($"{padding}Type: {criterion.Type}");
        }
        else if (criterion.Substitution != null)
        {
            await output.WriteLineAsync($"{padding}Substitution: {criterion.Substitution}");
        }
        else if (criterion.Criteria != null)
        {
            await output.WriteLineAsync($"{padding}Criteria: {criterion.Criteria}");
        }

        foreach (var subCriteria in criterion.Subcriteria ?? [])
        {
            await WriteCriterion(output, subCriteria, "  " + padding);
        }
    }
}
