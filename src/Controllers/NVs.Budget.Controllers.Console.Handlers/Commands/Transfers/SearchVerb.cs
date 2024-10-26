using System.Linq.Expressions;
using CommandLine;
using FluentResults;
using JetBrains.Annotations;
using MediatR;
using NVs.Budget.Application.Contracts.Entities.Budgeting;
using NVs.Budget.Application.Contracts.Errors.Accounting;
using NVs.Budget.Application.Contracts.Services;
using NVs.Budget.Application.Contracts.UseCases.Transfers;
using NVs.Budget.Controllers.Console.Contracts.Commands;
using NVs.Budget.Domain.Entities.Transactions;
using NVs.Budget.Infrastructure.IO.Console.Input.Criteria;
using NVs.Budget.Infrastructure.IO.Console.Output;
using Error = FluentResults.Error;

namespace NVs.Budget.Controllers.Console.Handlers.Commands.Transfers;

[Verb("search", HelpText = "Search for transfers within a budget")]
internal class SearchVerb : CriteriaBasedVerb
{
    [Option('b', "budget-id", Required = false, HelpText = "ID of a budget. Optional if user has only one budget, otherwise required")]
    public string BudgetId { get; [UsedImplicitly] set; } = string.Empty;

    [Option('c', "confidence", HelpText = "Register found transfers that matches confidence level")]
    public string? ConfidenceLevel { get; [UsedImplicitly] set; }
}

internal class SearchVerbHandler(IMediator mediator, IBudgetManager manager, ICriteriaParser parser, IResultWriter<Result> writer, IObjectWriter<Transfer> transfersWriter)
    : CriteriaBasedVerbHandler<SearchVerb, TrackedOperation>(parser, writer)
{
    protected override async Task<ExitCode> HandleInternal(SearchVerb request, Expression<Func<TrackedOperation, bool>> criteriaResultValue, CancellationToken ct)
    {
        DetectionAccuracy? accuracy = null;
        if (request.ConfidenceLevel is not null)
        {
            if (Enum.TryParse(request.ConfidenceLevel, out DetectionAccuracy level))
            {
                accuracy = level;
            }
            else
            {
                await Writer.Write(Result.Fail(new Error("Given apply is not a DetectionAccuracy!").WithMetadata("Value", request.ConfidenceLevel)), ct);
                return ExitCode.ArgumentsError;
            }
        }

        TrackedBudget? budget;
        var budgets = await manager.GetOwnedBudgets(ct);
        if (string.IsNullOrEmpty(request.BudgetId) && budgets.Count == 1)
        {
            budget = budgets.Single();
        }
        else
        {
            if (!Guid.TryParse(request.BudgetId, out var id))
            {
                await Writer.Write(Result.Fail(new Error("Given budget id is not a guid").WithMetadata("Value", request.BudgetId)), ct);
                return ExitCode.ArgumentsError;

            }

            budget = budgets.FirstOrDefault(b => b.Id == id);

            if (budget is null)
            {
                var fail = Result.Fail(new BudgetDoesNotExistError(id));
                await Writer.Write(fail, ct);
                return fail.ToExitCode();
            }
        }

        var found = await mediator.Send(new SearchTransfersCommand(budget, criteriaResultValue, accuracy), ct);
        await transfersWriter.Write(found, ct);

        return ExitCode.Success;
    }
}
