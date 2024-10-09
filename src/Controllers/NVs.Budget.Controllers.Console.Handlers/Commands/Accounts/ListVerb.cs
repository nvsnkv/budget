using CommandLine;
using MediatR;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Application.Contracts.UseCases.Accounts;
using NVs.Budget.Controllers.Console.Contracts.Commands;
using NVs.Budget.Controllers.Console.Contracts.IO.Output;

namespace NVs.Budget.Controllers.Console.Handlers.Commands.Accounts;

[Verb("list", true, HelpText = "Lists owned accounts for current user")]
internal class ListVerb : AbstractVerb;

internal class ListVerbHandler(IMediator mediator, IObjectWriter<TrackedBudget> writer) : IRequestHandler<ListVerb, ExitCode>
{
    public async Task<ExitCode> Handle(ListVerb request, CancellationToken cancellationToken)
    {
        var accounts = await mediator.Send(new ListOwnedAccountsQuery(), cancellationToken);
        await writer.Write(accounts, cancellationToken);

        return ExitCode.Success;
    }
}
