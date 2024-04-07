using CommandLine;
using MediatR;
using NVs.Budget.Controllers.Console.Contracts.Commands;

namespace NVs.Budget.Controllers.Console.Handlers.Commands;

internal class SuperVerbHandler<T>(IMediator mediator, Parser parser) : IRequestHandler<T, ExitCode> where T : SuperVerb
{
    public async Task<ExitCode> Handle(T request, CancellationToken cancellationToken)
    {
        var parserResult = parser.ParseArguments(request.Args, request.Verbs);
        var result = await parserResult.MapResult(o => mediator.Send(o, cancellationToken), errs => Task.FromResult((object?)-1));

        if (result is ExitCode value)
        {
            return value;
        }

        throw new InvalidOperationException("Unexpected object given!")
        {
            Data = { { "result", result } }
        };
    }
}
