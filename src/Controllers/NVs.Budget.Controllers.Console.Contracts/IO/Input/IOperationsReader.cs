using FluentResults;
using NVs.Budget.Application.Contracts.Entities.Accounting;

namespace NVs.Budget.Controllers.Console.Contracts.IO.Input;

public interface IOperationsReader
{
    IAsyncEnumerable<Result<UnregisteredOperation>> ReadUnregisteredOperations(StreamReader input, string name, CancellationToken ct);
}
