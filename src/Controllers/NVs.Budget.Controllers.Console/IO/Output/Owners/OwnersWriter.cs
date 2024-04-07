using Microsoft.Extensions.Options;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Controllers.Console.Contracts.IO.Output;

namespace NVs.Budget.Controllers.Console.IO.Output.Owners;

internal class OwnersWriter(IOutputStreamProvider streams, IOptions<OutputOptions> options)
{
    public async Task Write(TrackedOwner owner)
    {
        var stream = await streams.GetOutput(options.Value.OutputStreamName);
        await using var writer = new StreamWriter(stream);
        await writer.WriteLineAsync($"[{owner.Id}] {owner.Name} (ver {owner.Version})");
    }
}
