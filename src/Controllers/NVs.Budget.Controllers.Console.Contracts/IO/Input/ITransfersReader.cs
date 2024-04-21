using FluentResults;
using NVs.Budget.Application.Contracts.Entities.Accounting;

namespace NVs.Budget.Controllers.Console.Contracts.IO.Input;

public interface ITransfersReader
{
    IAsyncEnumerable<Result<UnregisteredTransfer>> ReadUnregisteredTransfers(StreamReader input, CancellationToken ct);
}
