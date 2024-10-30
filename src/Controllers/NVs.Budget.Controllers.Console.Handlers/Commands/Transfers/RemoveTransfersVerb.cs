using CommandLine;
using FluentResults;
using MediatR;
using NVs.Budget.Application.Contracts.UseCases.Transfers;
using NVs.Budget.Controllers.Console.Contracts.Commands;
using NVs.Budget.Infrastructure.IO.Console.Output;
using Error = FluentResults.Error;

namespace NVs.Budget.Controllers.Console.Handlers.Commands.Transfers;

[Verb("remove", HelpText = "Remove tracked transfer")]
internal class RemoveTransfersVerb : AbstractVerb
{
    [Value(0, MetaName = "source id", HelpText = "Source Id of a transfer that should be removed")]
    public IEnumerable<Guid>? SourceIds { get; set; }

    [Option('a', "all", HelpText = "Remove all tracked transfers")]
    public bool All {get; set;}
}

internal class RemoveTransfersVerbHandler(IMediator mediator, IResultWriter<Result> writer) : IRequestHandler<RemoveTransfersVerb, ExitCode>
{
    public async Task<ExitCode> Handle(RemoveTransfersVerb request, CancellationToken cancellationToken)
    {
        var ids = (request.SourceIds ?? Enumerable.Empty<Guid>()).ToList();
        if (ids.Count != 0 && request.All)
        {
            await writer.Write(Result.Fail(new Error("Please provide either list of ids or --all flag")), cancellationToken);
            return ExitCode.ArgumentsError;
        }



        var result = await mediator.Send(new RemoveTransfersCommand(ids.ToArray(), request.All), cancellationToken);
        return result.ToExitCode();
    }
}
