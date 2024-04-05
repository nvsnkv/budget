using FluentResults;
using Microsoft.Extensions.Options;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Controllers.Console.IO.Results;

namespace NVs.Budget.Controllers.Console.IO.Owners;

internal class OwnerResultWriter(OutputStreams outputStreams, IOptions<OutputOptions> options, OwnersWriter writer) : GenericResultWriter<Result<TrackedOwner>>(outputStreams, options)
{
    public override async Task Write(Result<TrackedOwner> response, CancellationToken ct)
    {
        await base.Write(response, ct);
        if (response.IsSuccess)
        {
            var owner = response.Value;
            await writer.Write(owner);
        }
    }
}
