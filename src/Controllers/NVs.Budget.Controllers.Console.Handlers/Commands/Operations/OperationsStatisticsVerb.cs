using System.Linq.Expressions;
using CommandLine;
using FluentResults;
using MediatR;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Application.Contracts.UseCases.Operations;
using NVs.Budget.Controllers.Console.Handlers.Utils;
using NVs.Budget.Domain.Aggregates;
using NVs.Budget.Infrastructure.IO.Console.Input;
using NVs.Budget.Infrastructure.IO.Console.Input.Criteria;
using NVs.Budget.Infrastructure.IO.Console.Output;

namespace NVs.Budget.Controllers.Console.Handlers.Commands.Operations;

[Verb("stats", HelpText = "Produces aggregated statistic using predefined set of aggregation rules")]
internal class OperationsStatisticsVerb : StatisticsVerb
{
    [Option('r', "ruleset", Required = true, HelpText = "Path to ruleset used for aggregation")]
    public string Ruleset { get; set; } = "";
}

internal class OperationsStatisticsVerbHandler(
    IMediator mediator,
    IInputStreamProvider inputStreamProvider,
    ILogbookCriteriaReader criteriaReader,
    ICriteriaParser parser,
    ILogbookWriter logbookWriter,
    IResultWriter<Result> writer,
    CronBasedNamedRangeSeriesBuilder seriesBuilder
) : StatisticsVerbHandlerBase<OperationsStatisticsVerb, TrackedOperation>(parser, logbookWriter, writer, seriesBuilder)
{
    protected override async Task<Result<CriteriaBasedLogbook>> GetLogbook(OperationsStatisticsVerb request, Expression<Func<TrackedOperation, bool>> criteriaResultValue, CancellationToken ct)
    {
        var input = await inputStreamProvider.GetInput(request.Ruleset);
        if (input.IsFailed)
        {
            return input.ToResult();
        }

        var criterion = await criteriaReader.ReadFrom(input.Value, ct);
        if (criterion.IsFailed)
        {
            return criterion.ToResult();
        }

        var query = new CalcOperationsStatisticsQuery(criterion.Value, criteriaResultValue);
        return await mediator.Send(query, ct);
    }
}
