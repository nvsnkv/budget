using FluentResults;
using Microsoft.Extensions.Options;
using NVs.Budget.Application.Contracts.Entities.Budgeting;
using NVs.Budget.Infrastructure.IO.Console.Options;
using NVs.Budget.Infrastructure.IO.Console.Output.Results;

namespace NVs.Budget.Infrastructure.IO.Console.Output.Owners;

internal class OwnerResultWriter(IOutputStreamProvider outputStreams, IOptionsSnapshot<OutputOptions> options, IObjectWriter<TrackedOwner> writer) : GenericResultWriter<Result<TrackedOwner>>(outputStreams, options)
{
    public override async Task Write(Result<TrackedOwner> response, CancellationToken ct)
    {
        await base.Write(response, ct);
        if (response.IsSuccess)
        {
            var owner = response.Value;
            await writer.Write(owner, ct);
        }
    }
}
