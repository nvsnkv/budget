using FluentResults;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Infrastructure.IO.Console.Options;

namespace NVs.Budget.Infrastructure.IO.Console.Input;

public interface IOperationsReader
{
    IAsyncEnumerable<Result<UnregisteredOperation>> ReadUnregisteredOperations(StreamReader input, SpecificCsvFileReadingOptions options, CancellationToken ct);

    IAsyncEnumerable<Result<TrackedOperation>> ReadTrackedOperation(StreamReader input, CancellationToken ct);
}
