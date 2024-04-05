using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Domain.Entities.Accounts;

namespace NVs.Budget.Controllers.Console.IO.Owners;

internal class OwnersWriter(OutputStreams streams)
{
    public Task Write(TrackedOwner owner) => streams.Out.WriteLineAsync($"[{owner.Id}] {owner.Name} (ver {owner.Version})");
}
