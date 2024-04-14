using Microsoft.Extensions.Options;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Controllers.Console.Contracts.IO.Output;

namespace NVs.Budget.Controllers.Console.IO.Output.Owners;

internal class OwnersWriter(IOutputStreamProvider streams, IOptions<OutputOptions> options) : IObjectWriter<TrackedOwner>
{
    public Task Write(TrackedOwner owner, CancellationToken ct) => Write([owner], ct);

    public async Task Write(IEnumerable<TrackedOwner> collection, CancellationToken ct)
    {
        var writer = await streams.GetOutput(options.Value.OutputStreamName);
        foreach (var owner in collection)
        {
            ct.ThrowIfCancellationRequested();
            await writer.WriteLineAsync($"[{owner.Id}] {owner.Name} (ver {owner.Version})");
        }
    }
}
