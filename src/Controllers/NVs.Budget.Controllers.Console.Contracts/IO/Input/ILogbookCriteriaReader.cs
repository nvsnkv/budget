using FluentResults;
using NVs.Budget.Domain.ValueObjects.Criteria;

namespace NVs.Budget.Controllers.Console.Contracts.IO.Input;

public interface ILogbookCriteriaReader
{
    Task<Result<Criterion>> ReadFrom(StreamReader reader, CancellationToken ct);
}
