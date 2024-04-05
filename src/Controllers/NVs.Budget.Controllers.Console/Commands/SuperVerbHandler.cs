using CommandLine;
using MediatR;

namespace NVs.Budget.Controllers.Console.Commands;

internal class SuperVerbHandler<T>(IMediator mediator, Parser parser) : IRequestHandler<T, int> where T : SuperVerb
{
    public async Task<int> Handle(T request, CancellationToken cancellationToken)
    {
        var parserResult = parser.ParseArguments(request.Args, request.Verbs);
        var result = await parserResult.MapResult(o => mediator.Send(o, cancellationToken), errs => Task.FromResult((object?)-1));

        if (result is int value)
        {
            return value;
        }

        throw new InvalidOperationException("Unexpected object given!")
        {
            Data = { { "result", result } }
        };
    }
}
