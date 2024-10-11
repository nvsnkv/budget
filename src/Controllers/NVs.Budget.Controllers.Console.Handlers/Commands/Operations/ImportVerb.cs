using CommandLine;
using FluentResults;
using JetBrains.Annotations;
using MediatR;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Application.Contracts.Options;
using NVs.Budget.Application.Contracts.Services;
using NVs.Budget.Application.Contracts.UseCases.Operations;
using NVs.Budget.Controllers.Console.Contracts.Commands;
using NVs.Budget.Infrastructure.IO.Console.Input;
using NVs.Budget.Infrastructure.IO.Console.Options;
using NVs.Budget.Infrastructure.IO.Console.Output;

namespace NVs.Budget.Controllers.Console.Handlers.Commands.Operations;

[Verb("import", HelpText = "Import operations from files")]
internal class ImportVerb : AbstractVerb
{
    [Option('b', "budget", Required = true, HelpText = "ID of a budget to import operations to")]
    public string BudgetId { get; [UsedImplicitly] set; } = string.Empty;

    [Option('d', "dir", Required = true, HelpText = "Directory with files to import")]
    public string? DirectoryPath { get; [UsedImplicitly] set; }

    [Option("transfers-confidence", Required = false, Default = DetectionAccuracy.Exact, HelpText = "Transfers detection accuracy (Exact or Likely)")]
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

        if (!Guid.TryParse(request.BudgetId, out var budgetId))
        {
            await resultWriter.Write(Result.Fail("Given ID is not a guid"), cancellationToken);
            return ExitCode.ArgumentsError;
        }

        var budgets = await manager.GetOwnedBudgets(cancellationToken);
        var budget = budgets.FirstOrDefault(b => b.Id == budgetId);
        if (budget is null)
        {
            await resultWriter.Write(Result.Fail("Budget with given id does not exists"), cancellationToken);
            return ExitCode.ArgumentsError;
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
