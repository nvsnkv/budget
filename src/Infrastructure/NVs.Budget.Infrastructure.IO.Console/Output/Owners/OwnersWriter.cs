using Microsoft.Extensions.Options;
using NVs.Budget.Application.Contracts.Entities.Budgeting;
using NVs.Budget.Infrastructure.IO.Console.Options;

namespace NVs.Budget.Infrastructure.IO.Console.Output.Owners;

internal class OwnersWriter(IOutputStreamProvider streams, IOptionsSnapshot<OutputOptions> options) : IObjectWriter<TrackedOwner>
{
    public Task Write(TrackedOwner criterion, CancellationToken ct) => Write(criterion, options.Value.OutputStreamName, ct);
    public Task Write(TrackedOwner criterion, string streamName, CancellationToken ct) => Write([criterion], streamName, ct);

    public Task Write(IEnumerable<TrackedOwner> collection, CancellationToken ct) => Write(collection, options.Value.OutputStreamName, ct);
    public async Task Write(IEnumerable<TrackedOwner> collection, string streamName, CancellationToken ct)
    {
        var writer = await streams.GetOutput(streamName);
        foreach (var owner in collection)
        {
            ct.ThrowIfCancellationRequested();
            await writer.WriteLineAsync($"[{owner.Id}] {owner.Name} (ver {owner.Version})");
        }

        await writer.FlushAsync(ct);
    }
}
