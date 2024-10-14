using CommandLine;
using FluentResults;
using MediatR;
using NVs.Budget.Application.Contracts.Errors.Accounting;
using NVs.Budget.Application.Contracts.Services;
using NVs.Budget.Application.Contracts.UseCases.Operations;
using NVs.Budget.Controllers.Console.Handlers.Utils;
using NVs.Budget.Domain.Aggregates;
using NVs.Budget.Infrastructure.IO.Console.Output;
using Error = FluentResults.Error;

namespace NVs.Budget.Controllers.Console.Handlers.Commands.Operations;

[Verb("stats", HelpText = "Produces aggregated statistic using predefined set of aggregation rules")]
internal class OperationsStatisticsVerb : StatisticsVerb
{
    [Option('b', "budget-id", Required = true)]
    public string BudgetId { get; set; } = string.Empty;
}

internal class OperationsStatisticsVerbHandler(
    IMediator mediator,
    IBudgetManager manager,
    ILogbookWriter logbookWriter,
    IResultWriter<Result> writer,
    CronBasedNamedRangeSeriesBuilder seriesBuilder
) : StatisticsVerbHandlerBase<OperationsStatisticsVerb>(logbookWriter, writer, seriesBuilder)
{
    protected override async Task<Result<CriteriaBasedLogbook>> GetLogbook(OperationsStatisticsVerb request,  CancellationToken ct)
    {
        if (!Guid.TryParse(request.BudgetId, out var id))
        {
            return Result.Fail(new Error("Given budget id is not a guid").WithMetadata("Value", request.BudgetId));
        }

        var budget = (await manager.GetOwnedBudgets(ct)).FirstOrDefault(b => b.Id == id);
        if (budget is null)
        {
            return Result.Fail(new BudgetDoesNotExistError(id));
        }

        var query = new CalcOperationsStatisticsQuery(budget.LogbookCriteria.GetCriterion(), o =>
            o.Timestamp >= request.From.ToUniversalTime()
            && o.Timestamp < request.Till.ToUniversalTime()
            && o.Budget.Id == id);

        return await mediator.Send(query, ct);
    }
}
