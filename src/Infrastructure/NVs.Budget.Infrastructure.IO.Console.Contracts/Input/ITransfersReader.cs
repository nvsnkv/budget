using FluentResults;
using NVs.Budget.Application.Contracts.Entities.Budgeting;

namespace NVs.Budget.Infrastructure.IO.Console.Input;

public interface ITransfersReader
{
    IAsyncEnumerable<Result<UnregisteredTransfer>> ReadUnregisteredTransfers(StreamReader input, CancellationToken ct);
}
