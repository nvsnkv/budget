using Microsoft.Extensions.Options;
using NVs.Budget.Application.Contracts.Entities.Budgeting;
using NVs.Budget.Application.Contracts.Results;
using NVs.Budget.Infrastructure.IO.Console.Options;
using NVs.Budget.Infrastructure.IO.Console.Output.Results;

namespace NVs.Budget.Infrastructure.IO.Console.Output.Operations;

internal class ImportResultWriter(
    IOutputStreamProvider streams,
    IOptionsSnapshot<OutputOptions> options,
    IObjectWriter<TrackedOperation> opWriter,
    IObjectWriter<TrackedTransfer> xferWriter) : GenericResultWriter<ImportResult>(streams, options)
{
    public override async Task Write(ImportResult result, CancellationToken ct)
    {
        await base.Write(result, ct);
        if (result.IsSuccess)
        {
            var writer = await OutputStreams.GetOutput(Options.Value.OutputStreamName);
            await writer.WriteLineAsync("Operations");

            foreach (var operation in result.Operations)
            {
                await opWriter.Write(operation, ct);
            }

            await writer.WriteLineAsync();
            await writer.WriteLineAsync("Transfers");

            foreach (var transfer in result.Transfers)
            {
                await xferWriter.Write(transfer, ct);
            }

            await writer.WriteLineAsync();
            await writer.WriteLineAsync("Duplicates");

            foreach (var duplicates in result.Duplicates)
            {
                foreach (var duplicate in duplicates)
                {
                    await opWriter.Write(duplicate, ct);
                }

                await writer.WriteLineAsync();
            }
        }
    }
}
