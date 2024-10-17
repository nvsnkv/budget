using AutoMapper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Options;
using NVs.Budget.Application.Contracts.Entities.Budgeting;
using NVs.Budget.Domain.Entities.Operations;
using NVs.Budget.Infrastructure.IO.Console.Models;
using NVs.Budget.Infrastructure.IO.Console.Options;

namespace NVs.Budget.Infrastructure.IO.Console.Output.Operations;

internal class TransfersWriter(IOutputStreamProvider streams, IOptionsSnapshot<OutputOptions> options, IMapper mapper, CsvConfiguration config, IObjectWriter<Operation> opWriter)
    : CsvObjectWriter<TrackedTransfer, CsvTransfer, CsvTrackedOperationClassMap>(streams, options, mapper, config)
{
    public override Task Write(IEnumerable<TrackedTransfer> collection, CancellationToken ct) => DoWrite(collection, WriteOperations, ct);

    private async Task<bool> WriteOperations(TrackedTransfer transfer, CancellationToken ct)
    {
        await opWriter.Write(transfer.Source, ct);
        await opWriter.Write(transfer.Sink, ct);

        return true;
    }
}
