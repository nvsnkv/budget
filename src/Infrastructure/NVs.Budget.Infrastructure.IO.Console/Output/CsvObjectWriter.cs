using AutoMapper;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Options;
using NVs.Budget.Infrastructure.IO.Console.Options;

namespace NVs.Budget.Infrastructure.IO.Console.Output;

internal abstract class CsvObjectWriter<T, TRow, TMap>(
    IOutputStreamProvider streams,
    IOptionsSnapshot<OutputOptions> options,
    IMapper mapper,
    CsvConfiguration config
) : IObjectWriter<T> where TMap : ClassMap
{
    protected IOutputStreamProvider Streams { get; } = streams;
    protected IOptionsSnapshot<OutputOptions> Options { get; } = options;

    public Task Write(T obj, CancellationToken ct) => Write([obj], ct);

    public virtual Task Write(IEnumerable<T> collection, CancellationToken ct) => DoWrite(collection, (_, _) => Task.FromResult(false), ct);

    protected async Task DoWrite(IEnumerable<T> collection, Func<T, CancellationToken, Task<bool>> func, CancellationToken ct)
    {
        var writer = await Streams.GetOutput(Options.Value.OutputStreamName);
        var csvWriter = new CsvWriter(writer, config, true);
        csvWriter.Context.RegisterClassMap<TMap>();

        csvWriter.WriteHeader<TRow>();
        await csvWriter.NextRecordAsync();

        foreach (var (t, row) in collection.Select(o => (o, mapper.Map<TRow>(o))))
        {
            csvWriter.WriteRecord(row);
            await csvWriter.NextRecordAsync();

            var newLineNeeded = await func(t, ct);
            if (newLineNeeded)
            {
                await writer.WriteLineAsync();
            }

            ct.ThrowIfCancellationRequested();
        }

        await csvWriter.FlushAsync();
    }
}
