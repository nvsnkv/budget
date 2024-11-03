using System.Linq.Expressions;
using CommandLine;
using FluentResults;
using JetBrains.Annotations;
using MediatR;
using NVs.Budget.Application.Contracts.Entities.Budgeting;
using NVs.Budget.Application.Contracts.Errors.Accounting;
using NVs.Budget.Application.Contracts.Services;
using NVs.Budget.Application.Contracts.UseCases.Operations;
using NVs.Budget.Controllers.Console.Contracts.Commands;
using NVs.Budget.Infrastructure.IO.Console.Input.Criteria;
using NVs.Budget.Infrastructure.IO.Console.Output;
using Error = FluentResults.Error;

namespace NVs.Budget.Controllers.Console.Handlers.Commands.Operations;

[Verb("retag", HelpText = "Retags operations using tagging criteria from config")]
internal class RetagVerb : CriteriaBasedVerb
{
    [Option('b', "budget-id", Required = false, HelpText = "ID of a budget. Optional if user has only one budget, otherwise required")]
    public string BudgetId { get; [UsedImplicitly] set; } = string.Empty;

    [Option("from-scratch", HelpText = "Clear old tags")]
    public bool FromScratch { get; [UsedImplicitly] set; }
}

internal class RetagVerbHandler(IMediator mediator, ICriteriaParser parser, IResultWriter<Result> writer, IBudgetManager manager) : CriteriaBasedVerbHandler<RetagVerb, TrackedOperation>(parser, writer)
{
    protected override async Task<ExitCode> HandleInternal(RetagVerb request, Expression<Func<TrackedOperation, bool>> criteriaResultValue, CancellationToken ct)
    {
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

        var command = new RetagOperationsCommand(criteriaResultValue, budget, request.FromScratch);
        var result = await mediator.Send(command, ct);
        return result.ToExitCode();
    }
}
