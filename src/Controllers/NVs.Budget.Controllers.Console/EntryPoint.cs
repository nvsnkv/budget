using CommandLine;
using CommandLine.Text;
using MediatR;
using Microsoft.Extensions.Hosting;
using NVs.Budget.Controllers.Console.Commands;
using NVs.Budget.Controllers.Console.IO;

namespace NVs.Budget.Controllers.Console;

internal class EntryPoint(Mediator mediator, Parser parser, OutputStreams streams) : IEntryPoint
{
    private static readonly Type[] SuperVerbTypes = typeof(SuperVerb).Assembly.GetTypes().Where(t => t.IsAssignableTo(typeof(SuperVerb))).ToArray();

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

                return (int)ExitCodes.UnexpectedResult;
            },
            async errs =>
            {
                var helpText = HelpText.AutoBuild(parsedResult);
                var errors = errs as Error[] ?? errs.ToArray();
                var isHelp = errors.IsHelp() || errors.IsVersion();
                var stream = isHelp ? streams.Out : streams.Error;

                await stream.WriteLineAsync(helpText);
                return (int)(isHelp ? ExitCodes.Success : ExitCodes.ArgumentsError);
            });
    }
}
