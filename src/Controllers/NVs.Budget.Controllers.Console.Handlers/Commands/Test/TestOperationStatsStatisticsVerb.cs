using CommandLine;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Options;
using NVs.Budget.Controllers.Console.Contracts.Commands;
using NVs.Budget.Controllers.Console.Contracts.IO.Input;
using NVs.Budget.Controllers.Console.Contracts.IO.Output;
using NVs.Budget.Controllers.Console.IO.Output;
using NVs.Budget.Domain.ValueObjects.Criteria;

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

    private async Task WriteCriterion(StreamWriter output, Criterion criterion, string padding)
    {
        await output.WriteLineAsync($"Description: {criterion.Description} [{criterion.Description.GetType().Name}]");
        foreach (var subCriteria in criterion.Subcriteria)
        {
            await WriteCriterion(output, subCriteria, "  " + padding);
        }
    }
}
