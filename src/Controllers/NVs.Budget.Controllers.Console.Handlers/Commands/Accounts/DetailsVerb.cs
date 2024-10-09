using CommandLine;
using FluentResults;
using MediatR;
using NVs.Budget.Controllers.Console.Contracts.Commands;
using NVs.Budget.Infrastructure.IO.Console.Output;

namespace NVs.Budget.Controllers.Console.Handlers.Commands.Accounts;

[Verb("details", HelpText = "Display details of specific budget")]
internal sealed class DetailsVerb: AbstractVerb
{
    [Value(0, Required = true, MetaName = "budget id")] public string Id { get; set; } = string.Empty;
}

internal class DetailsVerbHandler(IMediator mediator, IResultWriter<Result> writer) : IRequestHandler<DetailsVerb, ExitCode>
{
    public async Task<ExitCode> Handle(DetailsVerb request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.Id, out var id))
        {
            await writer.Write(Result.Fail("Input value is not a GUID"), cancellationToken);
            return ExitCode.ArgumentsError;
        }

        return ExitCode.Success;
    }
}
