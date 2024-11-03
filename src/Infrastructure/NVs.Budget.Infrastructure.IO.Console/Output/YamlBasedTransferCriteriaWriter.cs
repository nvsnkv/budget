using System.Collections;
using Microsoft.Extensions.Options;
using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Infrastructure.IO.Console.Options;

namespace NVs.Budget.Infrastructure.IO.Console.Output;

internal class YamlBasedTransferCriteriaWriter(IOutputStreamProvider streams, IOptionsSnapshot<OutputOptions> options) : IObjectWriter<TransferCriterion>
{
    public Task Write(TransferCriterion criterion, CancellationToken ct) => Write(criterion, options.Value.OutputStreamName, ct);
    public async Task Write(TransferCriterion criterion, string streamName, CancellationToken ct)
    {
        var writer = await streams.GetOutput(streamName);
        await writer.WriteLineAsync($"{criterion.Comment}:");
        await writer.WriteLineAsync($"  Accuracy: {criterion.Accuracy}");
        await writer.WriteLineAsync($"  Criterion: {criterion.Criterion}");
        await writer.FlushAsync(ct);
    }

    public Task Write(IEnumerable<TransferCriterion> collection, CancellationToken ct) => Write(collection, options.Value.OutputStreamName, ct);
    public async Task Write(IEnumerable<TransferCriterion> collection, string streamName, CancellationToken ct)
    {
        foreach (var criterion in collection)
        {
            await Write(criterion, streamName, ct);
        }
    }
}
