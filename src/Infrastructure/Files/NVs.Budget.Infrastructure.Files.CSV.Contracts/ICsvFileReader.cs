using FluentResults;
using NVs.Budget.Application.Contracts.Entities.Accounting;

namespace NVs.Budget.Infrastructure.Files.CSV.Contracts;

public interface ICsvFileReader
{
    IAsyncEnumerable<Result<UnregisteredOperation>> ReadUntrackedOperations(
        StreamReader reader,
        FileReadingSetting config,
        string clientTimeZoneId,
        CancellationToken ct);
};
