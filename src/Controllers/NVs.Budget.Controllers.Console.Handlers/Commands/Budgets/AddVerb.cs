using CommandLine;
using FluentResults;
using MediatR;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Application.Contracts.Services;
using NVs.Budget.Controllers.Console.Contracts.Commands;
using NVs.Budget.Infrastructure.IO.Console.Output;

namespace NVs.Budget.Controllers.Console.Handlers.Commands.Budgets;

[Verb("add", HelpText = "Adds new budget")]
internal class AddVerb : AbstractVerb
{
    [Value(0, MetaName = "Budget name", Required = true, HelpText = "Name of a budget")]
    public IEnumerable<string> Name { get; set; } = Enumerable.Empty<string>();
}

internal class AddVerbHandler(IBudgetManager manager, IResultWriter<Result<TrackedBudget>> writer) : IRequestHandler<AddVerb, ExitCode>
{
    public async Task<ExitCode> Handle(AddVerb request, CancellationToken cancellationToken)
    {
        var name = string.Join(' ', request.Name);
        var result = await manager.Register(new(name), cancellationToken);
        await writer.Write(result, cancellationToken);

        return result.ToExitCode();
    }
}
