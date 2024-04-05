using FluentResults;
using Microsoft.Extensions.Options;

namespace NVs.Budget.Controllers.Console.IO;

abstract class ResultWriter<T>(OutputStreams outputStreams, IOptions<OutputOptions> options) where T : IResultBase
{
    protected readonly OutputStreams OutputStreams = outputStreams;
    protected readonly IOptions<OutputOptions> Options = options;

    public virtual async Task Write(T response, CancellationToken ct)
    {
        await WriteErrors(response.Errors, ct);
        if (Options.Value.ShowSuccesses)
        {
            await WriteSuccesses(response.Successes, ct);
        }
    }

    protected async Task WriteSuccesses(List<ISuccess> successes, CancellationToken ct)
    {
        foreach (var success in successes)
        {
            await OutputStreams.Out.WriteLineAsync($"OK: {success.Message}");
            foreach (var (key, value) in success.Metadata)
            {
                ct.ThrowIfCancellationRequested();
                await OutputStreams.Out.WriteLineAsync($"  [{key}]: {value}");
            }
        }
    }

    protected async Task WriteErrors(List<IError> errors, CancellationToken ct)
    {
        foreach (var error in errors)
        {
            await WriterError(string.Empty, error, ct);
        }
    }

    private async Task WriterError(string prefix, IError error, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await OutputStreams.Error.WriteLineAsync($"{prefix}E: {error.Message}");
        foreach (var (key, value) in error.Metadata)
        {
            ct.ThrowIfCancellationRequested();
            await OutputStreams.Error.WriteLineAsync($"{prefix}  [{key}]: {value}");
        }

        foreach (var reason in error.Reasons)
        {
            await WriterError(prefix + "    ", reason, ct);
        }
    }
}
