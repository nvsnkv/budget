using FluentResults;
using NVs.Budget.Application.Contracts.Criteria;

namespace NVs.Budget.Infrastructure.IO.Console.Input;

public interface ITransferCriteriaReader
{
    IAsyncEnumerable<Result<TransferCriterion>> ReadFrom(StreamReader reader, CancellationToken ct);
}