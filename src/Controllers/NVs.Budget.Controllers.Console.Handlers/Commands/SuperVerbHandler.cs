using CommandLine;
using MediatR;
using Microsoft.Extensions.Configuration;
using NVs.Budget.Controllers.Console.Contracts.Commands;
using NVs.Budget.Controllers.Console.Contracts.IO.Options;
using NVs.Budget.Controllers.Console.IO.Output;

namespace NVs.Budget.Controllers.Console.Handlers.Commands;

internal class SuperVerbHandler<T>(IMediator mediator, Parser parser, IOutputOptionsChanger outputOptionsChanger) : IRequestHandler<T, ExitCode> where T : SuperVerb
{
    public async Task<ExitCode> Handle(T request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(request.OutputPath))
        {
            outputOptionsChanger.SetOutputStreamName(request.OutputPath);
        }

        if (!string.IsNullOrEmpty(request.ErrorsPath))
        {
            outputOptionsChanger.SetErrorStreamName(request.ErrorsPath);
        }

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
