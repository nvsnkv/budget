using FluentResults;
using NVs.Budget.Application.Contracts.Entities.Budgeting;

namespace NVs.Budget.Infrastructure.Files.CSV.Contracts;

public interface ICsvFileReader
{
    IAsyncEnumerable<Result<UnregisteredOperation>> ReadUntrackedOperations(StreamReader reader, FileReadingSetting config, CancellationToken ct);
};
