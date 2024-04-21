using CommandLine;
using CommandLine.Text;
using MediatR;
using Microsoft.Extensions.Options;
using NVs.Budget.Controllers.Console.Contracts.Commands;
using NVs.Budget.Controllers.Console.Contracts.IO.Output;
using NVs.Budget.Controllers.Console.IO.Output;

namespace NVs.Budget.Controllers.Console.Handlers;

internal class EntryPoint(IMediator mediator, Parser parser, IOutputStreamProvider streams, IOptionsSnapshot<OutputOptions> options) : IEntryPoint
{
    private static readonly Type[] SuperVerbTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes().Where(t => t.IsAssignableTo(typeof(SuperVerb)))).ToArray();

    public Task<int> Process(IEnumerable<string> args, CancellationToken ct)
    {
        var parsedResult = parser.ParseArguments(args, SuperVerbTypes);
        return parsedResult.MapResult(async obj =>
            {
                var result = await mediator.Send(obj, ct);
                if (result is int code)
                {
                    return code;
                }

                if (result is ExitCode exitCode)
                {
                    return (int)exitCode;
                }

                return (int)ExitCode.UnexpectedResult;
            },
            async errs =>
            {
                var helpText = HelpText.AutoBuild(parsedResult);
                var errors = errs as Error[] ?? errs.ToArray();
                var isHelp = errors.IsHelp() || errors.IsVersion();
                var writer = isHelp
                    ? await streams.GetOutput(options.Value.OutputStreamName)
                    : await streams.GetError(options.Value.ErrorStreamName);

                await writer.WriteLineAsync(helpText);
                await writer.FlushAsync(ct);
                return (int)(isHelp ? ExitCode.Success : ExitCode.ArgumentsError);
            });
    }
}
