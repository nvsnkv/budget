using CommandLine;
using MediatR;
using NVs.Budget.Application.Contracts.UseCases.Accounts;
using NVs.Budget.Controllers.Console.Contracts.Commands;

namespace NVs.Budget.Controllers.Console.Handlers.Commands.Accounts;

[Verb("merge", HelpText = "Merges source account into target - moves all operations from source into target and removes source account")]
internal class MergeAccountsVerb : AbstractVerb
{
    [Option("source", Required = true, HelpText = "Source account identifier")]
    public Guid SourceId { get; set; }

    [Option("target", Required = true, HelpText = "Target account identifier")]
    public Guid TargetId { get; set; }
}

internal class MergeAccountsVerbHandler(IMediator mediator) : IRequestHandler<MergeAccountsVerb, ExitCode>
{
    public async Task<ExitCode> Handle(MergeAccountsVerb request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new MergeAccountsCommand(request.SourceId, request.TargetId), cancellationToken);
        return result.ToExitCode();
    }
}
