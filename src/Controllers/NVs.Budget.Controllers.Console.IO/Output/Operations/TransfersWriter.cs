using AutoMapper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Options;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Controllers.Console.Contracts.IO.Output;
using NVs.Budget.Controllers.Console.IO.Models;
using NVs.Budget.Domain.Entities.Operations;

namespace NVs.Budget.Controllers.Console.IO.Output.Operations;

internal class TransfersWriter(IOutputStreamProvider streams, IOptions<OutputOptions> options, IMapper mapper, CsvConfiguration config, IObjectWriter<Operation> opWriter)
    : CsvObjectWriter<TrackedTransfer, CsvTransfer, CsvTrackedOperationClassMap>(streams, options, mapper, config)
{
    public override Task Write(IEnumerable<TrackedTransfer> collection, CancellationToken ct) => DoWrite(collection, WriteOperations, ct);

    private async Task WriteOperations(TrackedTransfer transfer, CancellationToken ct)
    {
        await opWriter.Write(transfer.Source, ct);
        await opWriter.Write(transfer.Sink, ct);
    }
}
