using CommandLine;
using MediatR;
using NVs.Budget.Application.Contracts.UseCases.Transfers;
using NVs.Budget.Controllers.Console.Contracts.Commands;

namespace NVs.Budget.Controllers.Console.Handlers.Commands.Transfers;

[Verb("remove", HelpText = "Remove tracked transfer")]
internal class RemoveTransfersVerb : AbstractVerb
{
    [Value(0, MetaName = "source id", HelpText = "Source Id of a transfer that should be removed")]
    public IEnumerable<Guid>? SourceIds { get; set; }
}

internal class RemoveTransfersVerbHandler(IMediator mediator) : IRequestHandler<RemoveTransfersVerb, ExitCode>
{
    public async Task<ExitCode> Handle(RemoveTransfersVerb request, CancellationToken cancellationToken)
    {
        var ids = request.SourceIds ?? Enumerable.Empty<Guid>();

        var result = await mediator.Send(new RemoveTransfersCommand(ids.ToArray()), cancellationToken);
        return result.ToExitCode();
    }
}
