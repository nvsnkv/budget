using CommandLine;
using CommandLine.Text;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Options;
using NVs.Budget.Controllers.Console.Contracts.Commands;
using NVs.Budget.Infrastructure.IO.Console.Input;
using NVs.Budget.Infrastructure.IO.Console.Options;
using NVs.Budget.Infrastructure.IO.Console.Output;
using Error = CommandLine.Error;

namespace NVs.Budget.Controllers.Console.Handlers;

internal class EntryPoint(
    IMediator mediator,
    Parser parser,
    IOutputStreamProvider streams,
    IOptionsSnapshot<OutputOptions> options,
    IInputStreamProvider inputs,
    IResultWriter<Result> resultWriter) : IEntryPoint
{
    private static readonly Type[] SuperVerbTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes().Where(t => t.IsAssignableTo(typeof(SuperVerb)))).ToArray();

    public async Task<int> Process(string[] args, CancellationToken ct)
    {
        if (args.Length == 0)
        {
            var reader = await inputs.GetInput();

            if (reader.IsFailed)
            {
                await resultWriter.Write(reader.ToResult(), ct);
                return (int)ExitCode.ArgumentsError;
            }

            do
            {
                var output = await streams.GetOutput(options.Value.OutputStreamName);
                await output.WriteAsync("> ");
                await output.FlushAsync(ct);

                var line = await reader.Value.ReadLineAsync(ct);
                if (ct.IsCancellationRequested)
                {
                    return (int)ExitCode.Cancelled;
                }

                args = line?.Split(' ').Where(s => !string.IsNullOrEmpty(s)).ToArray() ?? args;
                await ProcessArgs(args, ct);
                await streams.ReleaseStreamsAsync();

            } while (!ct.IsCancellationRequested);
        }

        return await ProcessArgs(args, ct);
    }

    private async Task<int> ProcessArgs(string[] args, CancellationToken ct)
    {
        var parsedResult = parser.ParseArguments(args, SuperVerbTypes);
        return await parsedResult.MapResult(async obj =>
            {
                //workaround to preserve options for subverbs
                if (obj is SuperVerb request)
                {
                    request.Args = args.Skip(1);
                }

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
