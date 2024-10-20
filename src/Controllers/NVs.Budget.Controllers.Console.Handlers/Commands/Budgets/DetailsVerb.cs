using CommandLine;
using FluentResults;
using MediatR;
using NVs.Budget.Application.Contracts.Entities.Budgeting;
using NVs.Budget.Application.Contracts.Services;
using NVs.Budget.Controllers.Console.Contracts.Commands;
using NVs.Budget.Infrastructure.IO.Console.Options;
using NVs.Budget.Infrastructure.IO.Console.Output;

namespace NVs.Budget.Controllers.Console.Handlers.Commands.Budgets;

[Verb("details", HelpText = "Display details of specific budget")]
internal sealed class DetailsVerb: AbstractVerb
{
    [Value(0, Required = true, MetaName = "Budget id")] public string Id { get; set; } = string.Empty;
}

internal class DetailsVerbHandler(
    IBudgetManager manager,
    IBudgetSpecificSettingsRepository csvSettingsRepo,
    IObjectWriter<TrackedBudget> budgetWriter,
    IObjectWriter<CsvReadingOptions> optionsWriter,
    IResultWriter<Result> writer) : IRequestHandler<DetailsVerb, ExitCode>
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

        var config = await csvSettingsRepo.GetReadingOptionsFor(budget, cancellationToken);
        await optionsWriter.Write(config, cancellationToken);

        return ExitCode.Success;
    }
}
