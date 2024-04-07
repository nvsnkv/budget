using Microsoft.Extensions.Options;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Application.Contracts.Results;
using NVs.Budget.Controllers.Console.Contracts.IO.Output;
using NVs.Budget.Controllers.Console.IO.Output.Results;

namespace NVs.Budget.Controllers.Console.IO.Output.Operations;

internal class ImportResultWriter(
    IOutputStreamProvider streams,
    IOptions<OutputOptions> options,
    IObjectWriter<TrackedOperation> opWriter,
    IObjectWriter<TrackedTransfer> xferWriter) : GenericResultWriter<ImportResult>(streams, options)
{
    public override async Task Write(ImportResult result, CancellationToken ct)
    {
        await base.Write(result, ct);
        if (result.IsSuccess)
        {
            foreach (var operation in result.Operations)
            {
                await opWriter.Write(operation, ct);
            }

            foreach (var transfer in result.Transfers)
            {
                await xferWriter.Write(transfer, ct);
            }
        }
    }
}
