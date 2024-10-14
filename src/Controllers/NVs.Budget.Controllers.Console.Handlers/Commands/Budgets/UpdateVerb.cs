using CommandLine;
using FluentResults;
using JetBrains.Annotations;
using MediatR;
using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Application.Contracts.Entities.Budgeting;
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
    public string? Name { get; [UsedImplicitly] set; }

    [Option("csv-reading-options", HelpText = "Path to YAML file with CSV reading options. If defined, options will be updated")]
    public string? CsvReadingOptionsPath { get; [UsedImplicitly] set; }

    [Option("tagging-criteria", HelpText = "Path to YAML file with tagging criteria. If defined, tagging criteria will be updated")]
    public string? TaggingCriteriaPath { get; [UsedImplicitly] set; }

    [Option("transfer-criteria", HelpText = "Path to YAML file with transfer criteria. If defined, transfer criteria will be updated")]
    public string? TransferCriteriaPath { get; [UsedImplicitly] set; }

    [Option("logbook-criteria", HelpText = "Path to YAML file with logbook criteria. If defined, transfer criteria will be updated")]
    public string? LogbookCriteriaPath { get; [UsedImplicitly] set; }
}

internal class UpdateVerbHandler(
    IInputStreamProvider input,
    ICsvReadingOptionsReader reader,
    ITransferCriteriaReader transferCriteriaReader,
    ILogbookCriteriaReader logbookCriteriaReader,
    ITaggingCriteriaReader taggingCriteriaReader,
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

        if (string.IsNullOrEmpty(request.Name) && string.IsNullOrEmpty(request.CsvReadingOptionsPath) && string.IsNullOrEmpty(request.TaggingCriteriaPath))
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

        var hasChanges = TryRename(request, budget);

        var changeTaggingCriteriaResult = await TryChangeTaggingCriteria(request, budget, cancellationToken);
        if (changeTaggingCriteriaResult.IsFailed)
        {
            return ExitCode.ArgumentsError;
        }

        hasChanges = hasChanges || changeTaggingCriteriaResult.Value;

        var changeTransferCriteriaResult = await TryChangeTransferCriteria(request, budget, cancellationToken);
        if (changeTransferCriteriaResult.IsFailed)
        {
            return ExitCode.ArgumentsError;
        }

        hasChanges = hasChanges || changeTransferCriteriaResult.Value;

        var changeLogbookCriteriaResult = await TryChangeLogbookCriteria(request, budget, cancellationToken);
        if (changeLogbookCriteriaResult.IsFailed)
        {
            return ExitCode.ArgumentsError;
        }

        hasChanges = hasChanges || changeLogbookCriteriaResult.Value;

        if (hasChanges)
        {
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

    private async Task<Result<bool>> TryChangeLogbookCriteria(UpdateVerb request, TrackedBudget budget, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.LogbookCriteriaPath))
        {
            return false;
        }

        if (!File.Exists(request.LogbookCriteriaPath))
        {
            var result = Result.Fail("Logbook criteria file does not exist");
            await resultWriter.Write(result, cancellationToken);
            return result;
        }

        var stream = await input.GetInput(request.LogbookCriteriaPath);
        if (!stream.IsSuccess)
        {
            var result = stream.ToResult();
            await resultWriter.Write(result, cancellationToken);
            return result;
        }

        var criteria = await logbookCriteriaReader.ReadFrom(stream.Value, cancellationToken);
        if (criteria.IsFailed)
        {
            var result = criteria.ToResult();
            await resultWriter.Write(result, cancellationToken);
            return result;
        }

        budget.SetLogbookCriteria(criteria.Value);
        return true;
    }

    private async Task<Result<bool>> TryChangeTransferCriteria(UpdateVerb request, TrackedBudget budget, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.TransferCriteriaPath))
        {
            return false;
        }

        if (!File.Exists(request.TransferCriteriaPath))
        {
            var result = Result.Fail("Transfer criteria file does not exist");
            await resultWriter.Write(result, cancellationToken);
            return result;
        }

        var stream = await input.GetInput(request.TransferCriteriaPath);
        if (!stream.IsSuccess)
        {
            var result = stream.ToResult();
            await resultWriter.Write(result, cancellationToken);
            return result;
        }

        var criteria = await transferCriteriaReader.ReadFrom(stream.Value, cancellationToken).ToListAsync(cancellationToken);
        var errors = criteria.Where(r => r.IsFailed).SelectMany(r => r.Errors);
        var values = criteria.Where(r => r.IsSuccess).Select(r => r.Value).ToList();

        if (values.Any())
        {
            budget.SetTransferCriteria(values);
            return Result.Ok(true).WithErrors(errors);
        }

        return Result.Fail(errors);
    }

    private async Task<Result<bool>> TryChangeTaggingCriteria(UpdateVerb request, TrackedBudget budget, CancellationToken cancellationToken)
    {
        List<TaggingCriterion>? taggingCriteria = null;
        if (!string.IsNullOrEmpty(request.TaggingCriteriaPath))
        {
            if (!File.Exists(request.TaggingCriteriaPath))
            {
                var result = Result.Fail("Tagging criteria file does not exist");
                await resultWriter.Write(result, cancellationToken);
                return result;
            }

            var stream = await input.GetInput(request.TaggingCriteriaPath);
            if (!stream.IsSuccess)
            {
                var result = stream.ToResult();
                await resultWriter.Write(result, cancellationToken);
                return result;
            }

            taggingCriteria = new List<TaggingCriterion>();
            await foreach (var rule in taggingCriteriaReader.ReadFrom(stream.Value, cancellationToken))
            {
                if (rule.IsSuccess)
                {
                    taggingCriteria.Add(rule.Value);
                }
                else
                {
                    await resultWriter.Write(rule.ToResult(), cancellationToken);
                }
            }

            if (!taggingCriteria.Any())
            {
                taggingCriteria = null;
            }
        }

        if (taggingCriteria is not null)
        {
            budget.SetTaggingCriteria(taggingCriteria);
        }

        return taggingCriteria is not null;
    }

    private static bool TryRename(UpdateVerb request, TrackedBudget budget)
    {
        if (!string.IsNullOrEmpty(request.Name))
        {
            budget.Rename(request.Name);
            return true;
        }

        return false;
    }
}
