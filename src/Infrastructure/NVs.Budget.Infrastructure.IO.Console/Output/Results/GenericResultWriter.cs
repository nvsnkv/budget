﻿using FluentResults;
using Microsoft.Extensions.Options;
using NVs.Budget.Infrastructure.IO.Console.Options;

namespace NVs.Budget.Infrastructure.IO.Console.Output.Results;

internal class GenericResultWriter<T>(IOutputStreamProvider streams, IOptionsSnapshot<OutputOptions> options): IResultWriter<T> where T : IResultBase
{
    protected readonly IOutputStreamProvider OutputStreams = streams;
    protected readonly IOptionsSnapshot<OutputOptions> Options = options;

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
            var writer = await OutputStreams.GetOutput(Options.Value.OutputStreamName);

            await writer.WriteLineAsync($"OK: {success.Message}");
            foreach (var (key, value) in success.Metadata)
            {
                ct.ThrowIfCancellationRequested();
                await writer.WriteLineAsync($"  [{key}]: {value}");
            }

            await writer.FlushAsync(ct);
        }
    }

    protected async Task WriteErrors(List<IError> errors, CancellationToken ct)
    {
        var writer = await OutputStreams.GetError(Options.Value.ErrorStreamName);

        foreach (var error in errors)
        {
            await WriterError(writer, string.Empty, error, ct);
        }

        await writer.FlushAsync(ct);
    }

    private async Task WriterError(StreamWriter writer, string prefix, IError error, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await writer.WriteLineAsync($"{prefix}E: {error.Message}");
        foreach (var (key, value) in error.Metadata)
        {
            ct.ThrowIfCancellationRequested();
            await writer.WriteLineAsync($"{prefix}  [{key}]: {value}");
        }

        foreach (var reason in error.Reasons)
        {
            await WriterError(writer, prefix + "    ", reason, ct);
        }
    }
}
