using FluentResults;
using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Application.Contracts.Entities.Budgeting;
using NVs.Budget.Infrastructure.IO.Console.Input.Criteria.Logbook;
using NVs.Budget.Utilities.Expressions;
using YamlDotNet.RepresentationModel;

namespace NVs.Budget.Infrastructure.IO.Console.Input;

internal class YamlBasedTaggingCriteriaReader(ReadableExpressionsParser parser) : YamlReader, ITaggingCriteriaReader
{
    public IAsyncEnumerable<Result<TaggingCriterion>> ReadFrom(StreamReader reader, CancellationToken ct)
    {
        var results = ReadSync(reader);
        return results.ToAsyncEnumerable();
    }

    private IEnumerable<Result<TaggingCriterion>> ReadSync(StreamReader reader)
    {
        var rootResult = LoadRootNodeFrom(reader);
        if (rootResult.IsFailed)
        {
            yield return rootResult.ToResult();
            yield break;
        }

        var root = rootResult.Value;
        foreach (var (tagKey, tagCriteria) in root)
        {
            var tag = ReadString(tagKey, EmptyPath);
            if (!tag.IsSuccess)
            {
                yield return tag.ToResult();
                continue;
            }

            if (tagCriteria is not YamlSequenceNode criteria)
            {
                yield return Result.Fail(new UnexpectedNodeTypeError(tagCriteria.GetType(), typeof(YamlSequenceNode), [tag.Value]));
                continue;
            }

            var tagExpr = parser.ParseUnaryConversion<TrackedOperation>(tag.Value);
            if (tagExpr.IsFailed)
            {
                yield return Result.Fail(new YamlParsingError("Failed to parse tagging expression", [tag.Value])).WithErrors(tagExpr.Errors);
                continue;
            }

            foreach (var criterionNode in criteria)
            {
                var criterion = ReadString(criterionNode, [tag.Value]);
                if (!criterion.IsSuccess)
                {
                    yield return criterion.ToResult();
                }
                else
                {
                    var criterionExpr = parser.ParseUnaryPredicate<TrackedOperation>(criterion.Value);
                    if (criterionExpr.IsFailed)
                    {
                        yield return Result.Fail(new YamlParsingError("Failed to parse tagging criterion", [tag.Value])).WithErrors(criterionExpr.Errors);
                    }


                    yield return new TaggingCriterion(tagExpr.Value, criterionExpr.Value);
                }
            }
        }
    }
}
