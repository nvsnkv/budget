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
using NVs.Budget.Infrastructure.IO.Console.Options;
using NVs.Budget.Infrastructure.IO.Console.Output;
using Error = FluentResults.Error;

namespace NVs.Budget.Controllers.Console.Handlers.Commands.Operations;

[Verb("import", HelpText = "Import operations from files")]
internal class ImportVerb : AbstractVerb
{
    [Option('b', "budget-id", Required = false, HelpText = "ID of a budget. Optional if user has only one budget, otherwise required")]
    public string BudgetId { get; [UsedImplicitly] set; } = string.Empty;

    [Option('d', "dir", Required = true, HelpText = "Directory with files to import")]
    public string? DirectoryPath { get; [UsedImplicitly] set; }

    [Option("confidence", Required = false, Default = DetectionAccuracy.Exact, HelpText = "Transfers detection accuracy (Exact or Likely)")]
    public DetectionAccuracy DetectionAccuracy { get; [UsedImplicitly] set; }
}

internal class ImportVerbHandler(
    IInputStreamProvider input,
    IResultWriter<Result> resultWriter,
    IBudgetSpecificSettingsRepository settingsRepo,
    IBudgetManager manager,
    IOperationsReader reader,
    IMediator mediator
) : IRequestHandler<ImportVerb, ExitCode>
{
    public async Task<ExitCode> Handle(ImportVerb request, CancellationToken cancellationToken)
    {
        if (!Directory.Exists(request.DirectoryPath))
        {
            return ExitCode.ArgumentsError;
        }

        var exitCodes = new HashSet<ExitCode> { ExitCode.Success };

        TrackedBudget? budget;
        var budgets = await manager.GetOwnedBudgets(cancellationToken);
        if (string.IsNullOrEmpty(request.BudgetId) && budgets.Count == 1)
        {
            budget = budgets.Single();
        }
        else
        {
            if (!Guid.TryParse(request.BudgetId, out var id))
            {
                await resultWriter.Write(Result.Fail(new Error("Given budget id is not a guid").WithMetadata("Value", request.BudgetId)), cancellationToken);
                return ExitCode.ArgumentsError;

            }

            budget = budgets.FirstOrDefault(b => b.Id == id);

            if (budget is null)
            {
                var fail = Result.Fail(new BudgetDoesNotExistError(id));
                await resultWriter.Write(fail, cancellationToken);
                return fail.ToExitCode();
            }
        }

        var csvOptions = await settingsRepo.GetReadingOptionsFor(budget, cancellationToken);

        var options = new ImportOptions(request.DetectionAccuracy);

        foreach (var file in Directory.EnumerateFiles(request.DirectoryPath))
        {
            var fileOptionsResult = csvOptions.GetFileOptionsFor(file);
            if (fileOptionsResult.IsFailed)
            {
                await resultWriter.Write(fileOptionsResult.ToResult(), cancellationToken);
                exitCodes.Add(ExitCode.OperationError);
                continue;
            }

            var fileStreamResult = await input.GetInput(file);
            if (fileStreamResult.IsFailed)
            {
                await resultWriter.Write(fileStreamResult.ToResult(), cancellationToken);
                exitCodes.Add(ExitCode.ArgumentsError);
            }
            else
            {
                var operations = reader.ReadUnregisteredOperations(fileStreamResult.Value, fileOptionsResult.Value, cancellationToken);
                var parsedOperations = operations.SelectAwait(async r =>
                {
                    if (r.IsSuccess) return r.Value;
                    await resultWriter.Write(r.ToResult(), cancellationToken);
                    exitCodes.Add(r.ToExitCode());
                    return null;

                }).Where(o => o is not null);

                var result = await mediator.Send(new ImportOperationsCommand(parsedOperations!,budget, options), cancellationToken);
                exitCodes.Add(result.ToExitCode());
            }
        }

        return exitCodes.Aggregate((r, e) => r | e);
    }
}
