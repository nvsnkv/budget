using CommandLine;
using FluentResults;
using MediatR;
using NVs.Budget.Application.Contracts.Services;
using NVs.Budget.Controllers.Console.Contracts.Commands;
using NVs.Budget.Infrastructure.IO.Console.Input;
using NVs.Budget.Infrastructure.IO.Console.Options;
using NVs.Budget.Infrastructure.IO.Console.Output;

namespace NVs.Budget.Controllers.Console.Handlers.Commands.Budgets;

[Verb("update", HelpText = "Updates budget-specific settings")]
internal class UpdateVerb : AbstractVerb
{
    [Value(0, Required = true, MetaName = "Budget id")] public string Id { get; set; } = string.Empty;

    [Option('n', "name", HelpText = "New name. If set, budget will be renamed")]
    public string? Name { get; set; }

    [Option("csv-reading-options", HelpText = "Path to YAML file with CSV reading options. If defined, options will be updated")]
    public string? CsvReadingOptionsPath { get; set; }
}

internal class UpdateVerbHandler(
    IInputStreamProvider input,
    ICsvReadingOptionsReader reader,
    IBudgetManager manager,
    IBudgetSpecificSettingsRepository repository,
    IResultWriter<Result> resultWriter
    ) : IRequestHandler<UpdateVerb, ExitCode>
{
    public async Task<ExitCode> Handle(UpdateVerb request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.Id, out var id))
        {
            await resultWriter.Write(Result.Fail("Input value is not a guid"), cancellationToken);
            return ExitCode.ArgumentsError;
        }

        if (string.IsNullOrEmpty(request.Name) && string.IsNullOrEmpty(request.CsvReadingOptionsPath))
        {
            await resultWriter.Write(Result.Fail("No options to update given"), cancellationToken);
            return ExitCode.ArgumentsError;
        }

        var budgets = await manager.GetOwnedBudgets(cancellationToken);
        var budget = budgets.FirstOrDefault(b => b.Id == id);
        if (budget is null)
        {
            await resultWriter.Write(Result.Fail("Budget with given id does not exist"), cancellationToken);
            return ExitCode.ArgumentsError;
        }

        if (!string.IsNullOrEmpty(request.Name))
        {
            budget.Rename(request.Name);
            var result = await manager.Update(budget, cancellationToken);
            if (!result.IsSuccess)
            {
                await resultWriter.Write(result, cancellationToken);
                return ExitCode.OperationError;
            }
        }

        if (!string.IsNullOrEmpty(request.CsvReadingOptionsPath))
        {
            if (!File.Exists(request.CsvReadingOptionsPath))
            {
                await resultWriter.Write(Result.Fail("CSV reading options file does not exist"), cancellationToken);
                return ExitCode.ArgumentsError;
            }

            var stream = await input.GetInput(request.CsvReadingOptionsPath);
            if (stream.IsFailed)
            {
                await resultWriter.Write(stream.ToResult(), cancellationToken);
                return ExitCode.ArgumentsError;
            }

            var config = await reader.ReadFrom(stream.Value, cancellationToken);
            if (config.IsFailed)
            {
                await resultWriter.Write(config.ToResult(), cancellationToken);
                return ExitCode.ArgumentsError;
            }

            var result = await repository.UpdateReadingOptionsFor(budget, config.Value, cancellationToken);
            if (result.IsFailed)
            {
                await resultWriter.Write(result, cancellationToken);
                return ExitCode.OperationError;
            }
        }

        return ExitCode.Success;
    }
}