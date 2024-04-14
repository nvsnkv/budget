using AutoMapper;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Options;
using NVs.Budget.Controllers.Console.Contracts.IO.Output;

namespace NVs.Budget.Controllers.Console.IO.Output.Operations;

internal abstract class CsvObjectWriter<T, TRow, TMap>(
    IOutputStreamProvider streams,
    IOptions<OutputOptions> options,
    IMapper mapper,
    CsvConfiguration config
) : IObjectWriter<T> where TMap : ClassMap
{

    public Task Write(T obj, CancellationToken ct) => Write([obj], ct);

    public virtual Task Write(IEnumerable<T> collection, CancellationToken ct) => DoWrite(collection, (_, _) => Task.CompletedTask, ct);

    protected async Task DoWrite(IEnumerable<T> collection, Func<T, CancellationToken, Task> func, CancellationToken ct)
    {
        var writer = await streams.GetOutput(options.Value.OutputStreamName);
        var csvWriter = new CsvWriter(writer, config, true);
        csvWriter.Context.RegisterClassMap<TMap>();

        foreach (var (t, row) in collection.Select(o => (o, mapper.Map<TRow>(o))))
        {
            csvWriter.WriteRecord(row);
            await func(t, ct);
            ct.ThrowIfCancellationRequested();
        }
    }
}