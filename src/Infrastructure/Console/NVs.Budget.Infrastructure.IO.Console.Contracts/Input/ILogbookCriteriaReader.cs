using FluentResults;
using NVs.Budget.Application.Contracts.Criteria;

namespace NVs.Budget.Infrastructure.IO.Console.Input;

public interface ILogbookCriteriaReader
{
    Task<Result<LogbookCriteria>> ReadFrom(StreamReader reader, CancellationToken ct);
}
