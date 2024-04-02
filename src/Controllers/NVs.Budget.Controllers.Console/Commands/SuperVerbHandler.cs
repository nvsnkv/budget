using CommandLine;
using MediatR;

namespace NVs.Budget.Controllers.Console.Commands;

internal class SuperVerbHandler : IRequestHandler<SuperVerb, int>
{
    private readonly IMediator _mediator;
    private readonly Parser _parser;

    public SuperVerbHandler(IMediator mediator, Parser parser)
    {
        _mediator = mediator;
        _parser = parser;
    }

    public async Task<int> Handle(SuperVerb request, CancellationToken cancellationToken)
    {
        var parserResult = _parser.ParseArguments(request.Args, request.Verbs);
        var result = await parserResult.MapResult(o => _mediator.Send(o, cancellationToken), errs => Task.FromResult((object?)-1));

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
