using CommandLine;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Options;
using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Application.Contracts.Entities.Budgeting;
using NVs.Budget.Application.Contracts.Services;
using NVs.Budget.Controllers.Console.Contracts.Commands;
using NVs.Budget.Infrastructure.IO.Console.Options;
using NVs.Budget.Infrastructure.IO.Console.Output;

namespace NVs.Budget.Controllers.Console.Handlers.Commands.Budgets;

[Verb("details", HelpText = "Display details of specific budget")]
internal sealed class DetailsVerb : BudgetDetailsVerb;

internal class DetailsVerbHandler(
    IBudgetManager manager,
    IBudgetSpecificSettingsRepository csvSettingsRepo,
    IObjectWriter<TrackedBudget> budgetWriter,
    IObjectWriter<CsvReadingOptions> optionsWriter,
    IObjectWriter<LogbookCriteria> logbookWriter,
    IObjectWriter<TransferCriterion> transferWriter,
    IObjectWriter<TaggingCriterion> taggingWriter,
    IResultWriter<Result> writer,
    IOutputStreamProvider streamProvider,
    IOptionsSnapshot<OutputOptions> outputOptions) : IRequestHandler<DetailsVerb, ExitCode>
{
    public async Task<ExitCode> Handle(DetailsVerb request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.Id, out var id))
        {
            await writer.Write(Result.Fail("Input value is not a guid"), cancellationToken);
            return ExitCode.ArgumentsError;
        }

        var budgets = await manager.GetOwnedBudgets(cancellationToken);
        var budget = budgets.FirstOrDefault(b => b.Id == id);
        if (budget is null)
        {
            await writer.Write(Result.Fail("Budget with given id does not exists"), cancellationToken);
            return ExitCode.ArgumentsError;
        }

        await budgetWriter.Write(budget, cancellationToken);
        var output = await streamProvider.GetOutput(outputOptions.Value.OutputStreamName);
        await output.WriteLineAsync($"Logbook: {request.LogbookCriteriaPath}");
        await output.WriteLineAsync($"Transfers: {request.TransferCriteriaPath}");
        await output.WriteLineAsync($"Tags: {request.TaggingCriteriaPath}");
        await output.WriteLineAsync($"CSV: {request.CsvReadingOptionsPath}");


        await logbookWriter.Write(budget.LogbookCriteria, request.LogbookCriteriaPath!, cancellationToken);
        await transferWriter.Write(budget.TransferCriteria, request.TransferCriteriaPath!, cancellationToken);
        await taggingWriter.Write(budget.TaggingCriteria, request.TaggingCriteriaPath!, cancellationToken);

        var config = await csvSettingsRepo.GetReadingOptionsFor(budget, cancellationToken);
        await optionsWriter.Write(config, request.CsvReadingOptionsPath!, cancellationToken);

        return ExitCode.Success;
    }
}
