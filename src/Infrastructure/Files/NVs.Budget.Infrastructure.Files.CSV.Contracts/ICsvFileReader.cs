using NVs.Budget.Application.Contracts.Entities.Budgeting;

namespace NVs.Budget.Infrastructure.Files.CSV.Contracts;

public interface ICsvFileReader
{
    IAsyncEnumerable<UnregisteredOperation> ReadUntrackedOperations(StreamReader reader, FileReadingSetting config, CancellationToken ct);
    IAsyncEnumerable<TrackedOperation> ReadTrackedOperations(StreamReader reader, CancellationToken ct);
};
