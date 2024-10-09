using FluentResults;
using NVs.Budget.Application.Contracts.Entities.Accounting;

namespace NVs.Budget.Infrastructure.IO.Console.Input;

public interface IOperationsReader
{
    IAsyncEnumerable<Result<UnregisteredOperation>> ReadUnregisteredOperations(StreamReader input, string name, CancellationToken ct);

    IAsyncEnumerable<Result<TrackedOperation>> ReadTrackedOperation(StreamReader input, CancellationToken ct);
}
