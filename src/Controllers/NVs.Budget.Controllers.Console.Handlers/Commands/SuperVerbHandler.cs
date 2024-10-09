using CommandLine;
using CommandLine.Text;
using MediatR;
using Microsoft.Extensions.Options;
using NVs.Budget.Controllers.Console.Contracts.Commands;
using NVs.Budget.Controllers.Console.Contracts.IO.Options;
using NVs.Budget.Controllers.Console.Contracts.IO.Output;
using NVs.Budget.Controllers.Console.IO.Output;

namespace NVs.Budget.Controllers.Console.Handlers.Commands;

internal class SuperVerbHandler<T>(IMediator mediator, Parser parser, IOutputOptionsChanger outputOptionsChanger, IOutputStreamProvider streams, IOptionsSnapshot<OutputOptions> options) : IRequestHandler<T, ExitCode> where T : SuperVerb
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
        var result = await parserResult.MapResult(o => mediator.Send(o, cancellationToken), async errs =>
        {
            var helpText = HelpText.AutoBuild(parserResult);
            var errors = errs as Error[] ?? errs.ToArray();
            var isHelp = errors.IsHelp() || errors.IsVersion();
            var writer = isHelp
                ? await streams.GetOutput(options.Value.OutputStreamName)
                : await streams.GetError(options.Value.ErrorStreamName);

            await writer.WriteLineAsync(helpText);
            await writer.FlushAsync(cancellationToken);
            return (object?)(isHelp ? ExitCode.Success : ExitCode.ArgumentsError);
        });

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
