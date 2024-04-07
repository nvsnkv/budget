using AutoMapper;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Options;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Controllers.Console.Contracts.IO.Output;
using NVs.Budget.Domain.Entities.Operations;

namespace NVs.Budget.Controllers.Console.IO.Output.Operations;

internal class TransfersWriter(IOutputStreamProvider streams, IOptions<OutputOptions> options, IMapper mapper, CsvConfiguration config, IObjectWriter<Operation> opWriter)
    : CsvObjectWriter<TrackedTransfer, CsvTransfer, CsvTrackedOperationClassMap>(streams, options, mapper, config)
{
    public override async Task Write(TrackedTransfer obj, CancellationToken ct)
    {
        await base.Write(obj, ct);
        await opWriter.Write(obj.Source, ct);
        await opWriter.Write(obj.Sink, ct);

    }
}

internal abstract class CsvObjectWriter<T, TRow, TMap>(
    IOutputStreamProvider streams,
    IOptions<OutputOptions> options,
    IMapper mapper,
    CsvConfiguration config
) : IObjectWriter<T>, IDisposable, IAsyncDisposable where TMap : ClassMap
{
    private volatile CsvWriter? _writer;
    public virtual async Task Write(T obj, CancellationToken ct)
    {
        if (_writer is null)
        {
            var outStream = await streams.GetOutput(options.Value.OutputStreamName);
            var stream = new StreamWriter(outStream);
            _writer = new CsvWriter(stream, config);
            _writer.Context.RegisterClassMap<TMap>();
        }

        _writer.WriteRecord(mapper.Map<TRow>(obj));
    }

    public void Dispose()
    {
        _writer?.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        if (_writer != null) await _writer.DisposeAsync();
    }
}
