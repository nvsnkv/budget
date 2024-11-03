using CommandLine;
using FluentResults;
using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.Options;
using NVs.Budget.Application.Contracts.Entities;
using NVs.Budget.Application.Contracts.Entities.Budgeting;
using NVs.Budget.Application.Contracts.Services;
using NVs.Budget.Controllers.Console.Contracts.Commands;
using NVs.Budget.Domain.Entities.Operations;
using NVs.Budget.Infrastructure.IO.Console.Input;
using NVs.Budget.Infrastructure.IO.Console.Options;
using NVs.Budget.Infrastructure.IO.Console.Output;

namespace NVs.Budget.Controllers.Console.Handlers.Commands.Test;

[Verb("import", HelpText = "Test CsvReadingOptions for particular file")]
internal class TestImportVerb : AbstractVerb
{
    [Option('f', "file", Required = true, HelpText = "Incoming file to test")]
    public string? FilePath { get; [UsedImplicitly] set; }

    [Option('b', "budget", Required = true, HelpText = "ID of a budget to import operations to")]
    public string BudgetId { get; [UsedImplicitly] set; } = string.Empty;
}

internal class TestImportVerbHandler(
    IBudgetManager manager,
    IBudgetSpecificSettingsRepository settingsRepository,
    IInputStreamProvider input,
    IOperationsReader reader,
    IOutputStreamProvider output,
    IOptionsSnapshot<OutputOptions> outputOptions,
    IResultWriter<Result> resultWriter,
    IObjectWriter<Operation> objectWriter,
    IUser user) : IRequestHandler<TestImportVerb, ExitCode>
{
    public async Task<ExitCode> Handle(TestImportVerb request, CancellationToken cancellationToken)
    {
        var filePath = request.FilePath ?? string.Empty;
        var streamResult = await input.GetInput(filePath);
        if (streamResult.IsFailed)
        {
            await resultWriter.Write(streamResult.ToResult(), cancellationToken);
            return ExitCode.ArgumentsError;
        }

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

        var settings = await settingsRepository.GetReadingOptionsFor(budget, cancellationToken);
        var fileOptionsResult = settings.GetFileOptionsFor(filePath);
        if (fileOptionsResult.IsFailed)
        {
            await resultWriter.Write(fileOptionsResult.ToResult(), cancellationToken);
            return ExitCode.OperationError;
        }

        var successes = new List<Operation>();
        var errors = new List<Result>();

        var ops = reader.ReadUnregisteredOperations(streamResult.Value, fileOptionsResult.Value, cancellationToken);

        await foreach (var result in ops)
        {
            if (result.IsSuccess)
            {
                successes.Add(CreateOperationFrom(result.Value));
            }
            else
            {
                errors.Add(result.ToResult());
            }
        }

        foreach (var error in errors)
        {
            await resultWriter.Write(error, cancellationToken);
        }

        await objectWriter.Write(successes, cancellationToken);

        var writer = await output.GetOutput(outputOptions.Value.OutputStreamName);
        await writer.WriteLineAsync();
        await writer.WriteLineAsync($"Total; {successes.Count + errors.Count}; Successes; {successes.Count}; Errors; {errors.Count}");
        await writer.FlushAsync(cancellationToken);

        return ExitCode.Success;
    }

    private Operation CreateOperationFrom(UnregisteredOperation unregistered)
    {
        return new Operation(
            Guid.Empty,
            unregistered.Timestamp,
            unregistered.Amount,
            unregistered.Description,
            new Domain.Entities.Accounts.Budget(Guid.Empty, "fake budget", [user.AsOwner()]),
            [],
            unregistered.Attributes
        );
    }
}
