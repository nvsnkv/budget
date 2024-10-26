using System.Linq.Expressions;
using CommandLine;
using FluentResults;
using JetBrains.Annotations;
using MediatR;
using NVs.Budget.Application.Contracts.Entities.Budgeting;
using NVs.Budget.Application.Contracts.Errors.Accounting;
using NVs.Budget.Application.Contracts.Options;
using NVs.Budget.Application.Contracts.Services;
using NVs.Budget.Application.Contracts.UseCases.Operations;
using NVs.Budget.Controllers.Console.Contracts.Commands;
using NVs.Budget.Infrastructure.IO.Console.Input;
using NVs.Budget.Infrastructure.IO.Console.Input.Criteria;
using NVs.Budget.Infrastructure.IO.Console.Output;
using Error = FluentResults.Error;

namespace NVs.Budget.Controllers.Console.Handlers.Commands.Operations;

[Verb("update", HelpText = "Updates tracked operations from the given input")]
internal class UpdateVerb : CriteriaBasedVerb
{
    [Option('b', "budget-id", Required = false, HelpText = "ID of a budget. Optional if user has only one budget, otherwise required")]
    public string BudgetId { get; [UsedImplicitly] set; } = string.Empty;

    [Option('f', "file", HelpText = "Path to file with update content. If value is not specified, app will use standard input")]
    public string? FilePath { get; [UsedImplicitly] set; }

    [Option('c', "confidence", HelpText = "Register found transfers that matches confidence level")]
    public string? ConfidenceLevel { get; [UsedImplicitly] set; }
}

internal class UpdateVerbHandler(IMediator mediator, IBudgetManager manager, IInputStreamProvider streams, IOperationsReader reader, ICriteriaParser parser, IResultWriter<Result> writer) : CriteriaBasedVerbHandler<UpdateVerb, TrackedOperation>(parser, writer)
{
    protected override async Task<ExitCode> HandleInternal(UpdateVerb request, Expression<Func<TrackedOperation, bool>> criteriaResultValue, CancellationToken ct)
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
                await Writer.Write(Result.Fail(new Error("Given budget id is not a guid").WithMetadata("Value", request.BudgetId)), ct);
                return ExitCode.ArgumentsError;
            }

            budget = budgets.FirstOrDefault(b => b.Id == id);

            if (budget is null)
            {
                await Writer.Write(Result.Fail(new BudgetDoesNotExistError(id)), ct);
                return ExitCode.ArgumentsError;
            }
        }

        var steamReader = await streams.GetInput(request.FilePath ?? string.Empty);
        if (!steamReader.IsSuccess)
        {
            await Writer.Write(steamReader.ToResult(), ct);
            return ExitCode.ArgumentsError;
        }

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


        var exitCodes = new HashSet<ExitCode>();
        var operations = reader.ReadTrackedOperation(steamReader.Value, ct).SelectAwait(async r =>
        {
            if (r.IsSuccess) return r.Value;
            await Writer.Write(r.ToResult(), ct);
            exitCodes.Add(r.ToExitCode());
            return null;
        }).Where(o => o is not null);

        var result = await mediator.Send(new UpdateOperationsCommand(operations!, budget, new(accuracy, TaggingMode.FromScratch)), ct);
        exitCodes.Add(result.ToExitCode());

        return exitCodes.Aggregate((r, e) => r | e);
    }
}
