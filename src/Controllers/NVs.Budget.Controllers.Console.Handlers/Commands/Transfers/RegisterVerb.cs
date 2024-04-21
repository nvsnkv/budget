using CommandLine;
using FluentResults;
using MediatR;
using NVs.Budget.Application.Contracts.UseCases.Transfers;
using NVs.Budget.Controllers.Console.Contracts.Commands;
using NVs.Budget.Controllers.Console.Contracts.IO.Input;
using NVs.Budget.Controllers.Console.Contracts.IO.Output;

namespace NVs.Budget.Controllers.Console.Handlers.Commands.Transfers;

[Verb("register", HelpText = "Register transfers manually")]
internal class RegisterVerb : AbstractVerb
{
    [Option('f', "file", HelpText = "Path to file with transfers to register. If value is not specified, app will use standard input")]
    public string? FilePath { get; set; }
}


internal class RegisterVerbHandler(IMediator mediator, IInputStreamProvider streams, ITransfersReader reader, IResultWriter<Result> writer) : IRequestHandler<RegisterVerb, ExitCode>
{
    public async Task<ExitCode> Handle(RegisterVerb request, CancellationToken cancellationToken)
    {
        var steamReader = await streams.GetInput(request.FilePath ?? string.Empty);
        if (!steamReader.IsSuccess)
        {
            await writer.Write(steamReader.ToResult(), cancellationToken);
            return ExitCode.ArgumentsError;
        }

        var exitCodes = new HashSet<ExitCode>();
        var transfers = reader.ReadUnregisteredTransfers(steamReader.Value, cancellationToken).SelectAwait(async r =>
        {
            if (r.IsSuccess) return r.Value;
            await writer.Write(r.ToResult(), cancellationToken);
            exitCodes.Add(r.ToExitCode());
            return null;
        }).Where(o => o is not null);

        var result = await mediator.Send(new RegisterTransfersCommand(transfers!), cancellationToken);
        exitCodes.Add(result.ToExitCode());

        return exitCodes.Aggregate((r, e) => r | e);
    }
}
