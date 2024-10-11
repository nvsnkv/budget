using FluentResults;
using NVs.Budget.Application.Contracts.Criteria;

namespace NVs.Budget.Infrastructure.IO.Console.Input;

public interface ITaggingRulesReader
{
    IAsyncEnumerable<Result<TaggingRule>> ReadFrom(StreamReader reader, CancellationToken ct);
}
