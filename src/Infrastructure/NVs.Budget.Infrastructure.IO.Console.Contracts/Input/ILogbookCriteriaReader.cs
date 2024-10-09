using FluentResults;
using NVs.Budget.Domain.ValueObjects.Criteria;

namespace NVs.Budget.Infrastructure.IO.Console.Input;

public interface ILogbookCriteriaReader
{
    Task<Result<Criterion>> ReadFrom(StreamReader reader, CancellationToken ct);
}
