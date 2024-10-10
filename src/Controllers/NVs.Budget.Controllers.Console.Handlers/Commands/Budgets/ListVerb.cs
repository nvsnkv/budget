using CommandLine;
using MediatR;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Application.Contracts.UseCases.Accounts;
using NVs.Budget.Controllers.Console.Contracts.Commands;
using NVs.Budget.Infrastructure.IO.Console.Output;

namespace NVs.Budget.Controllers.Console.Handlers.Commands.Budgets;

[Verb("list", true, HelpText = "Lists owned budgets for current user")]
internal class ListVerb : AbstractVerb;

internal class ListVerbHandler(IMediator mediator, IObjectWriter<TrackedBudget> writer) : IRequestHandler<ListVerb, ExitCode>
{
    public async Task<ExitCode> Handle(ListVerb request, CancellationToken cancellationToken)
    {
        var budgets = await mediator.Send(new ListOwnedBudgetsQuery(), cancellationToken);
        await writer.Write(budgets, cancellationToken);

        return ExitCode.Success;
    }
}
