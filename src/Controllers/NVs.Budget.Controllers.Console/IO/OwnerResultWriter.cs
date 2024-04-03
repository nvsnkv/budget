using FluentResults;
using Microsoft.Extensions.Options;
using NVs.Budget.Application.Contracts.Entities.Accounting;

namespace NVs.Budget.Controllers.Console.IO;

internal class OwnerResultWriter(OutputStreams outputStreams, IOptionsSnapshot<OutputOptions> options) : ResultWriter<Result<TrackedOwner>>(outputStreams, options)
{
    public override async Task Write(Result<TrackedOwner> response, CancellationToken ct)
    {
        await base.Write(response, ct);
        if (response.IsSuccess)
        {
            var owner = response.Value;
            await OutputStreams.Out.WriteLineAsync($"[{owner.Id}] {owner.Name} (ver {owner.Version})");
        }
    }
}
