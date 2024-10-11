using FluentResults;
using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Infrastructure.IO.Console.Input.Criteria.Logbook;
using YamlDotNet.RepresentationModel;

namespace NVs.Budget.Infrastructure.IO.Console.Input;

internal class YamlBasedTaggingCriteriaReader : YamlReader, ITaggingCriteriaReader
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

            foreach (var criterionNode in criteria)
            {
                var criterion = ReadString(criterionNode, [tag.Value]);
                if (!criterion.IsSuccess)
                {
                    yield return criterion.ToResult();
                }
                else
                {
                    yield return new TaggingCriterion(tag.Value, criterion.Value);
                }
            }
        }
    }
}
