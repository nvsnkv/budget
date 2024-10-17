using FluentResults;
using NVs.Budget.Application.Contracts.Criteria;

namespace NVs.Budget.Infrastructure.IO.Console.Input;

public interface ITaggingCriteriaReader
{
    IAsyncEnumerable<Result<TaggingCriterion>> ReadFrom(StreamReader reader, CancellationToken ct);
}
