using CommandLine;
using FluentResults;
using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.Options;
using NVs.Budget.Application.Contracts.Entities.Budgeting;
using NVs.Budget.Application.Contracts.Errors.Accounting;
using NVs.Budget.Application.Contracts.Services;
using NVs.Budget.Application.Contracts.UseCases.Operations;
using NVs.Budget.Controllers.Console.Handlers.Utils;
using NVs.Budget.Domain.Aggregates;
using NVs.Budget.Infrastructure.IO.Console.Options;
using NVs.Budget.Infrastructure.IO.Console.Output;
using Error = FluentResults.Error;

namespace NVs.Budget.Controllers.Console.Handlers.Commands.Operations;

[Verb("stats", HelpText = "Produces aggregated statistic using predefined set of aggregation rules")]
internal class OperationsStatisticsVerb : StatisticsVerb
{
    [Option('b', "budget-id", Required = false, HelpText = "ID of a budget. Optional if user has only one budget, otherwise required")]
    public string BudgetId { get; [UsedImplicitly] set; } = string.Empty;
}

internal class OperationsStatisticsVerbHandler(
    IMediator mediator,
    IBudgetManager manager,
    ILogbookWriter logbookWriter,
    IResultWriter<Result> writer,
    IOutputStreamProvider outputs,
    IOptionsSnapshot<OutputOptions> options,
    CronBasedNamedRangeSeriesBuilder seriesBuilder
) : StatisticsVerbHandlerBase<OperationsStatisticsVerb>(logbookWriter, writer, seriesBuilder, outputs, options.Value)
{
    protected override async Task<Result<CriteriaBasedLogbook>> GetLogbook(OperationsStatisticsVerb request,  CancellationToken ct)
    {
        TrackedBudget? budget;
        var budgets = await manager.GetOwnedBudgets(ct);
        if (budgets.Count == 1)
        {
            budget = budgets.Single();
        }
        else
        {
            if (!Guid.TryParse(request.BudgetId, out var id))
            {
                return Result.Fail(new Error("Given budget id is not a guid").WithMetadata("Value", request.BudgetId));
            }

            budget = budgets.FirstOrDefault(b => b.Id == id);

            if (budget is null)
            {
                return Result.Fail(new BudgetDoesNotExistError(id));
            }
        }

        var query = new CalcOperationsStatisticsQuery(budget.LogbookCriteria.GetCriterion(), o =>
            o.Timestamp >= request.From.ToUniversalTime()
            && o.Timestamp < request.Till.ToUniversalTime()
            && o.Budget.Id == budget.Id);

        return await mediator.Send(query, ct);
    }
}
