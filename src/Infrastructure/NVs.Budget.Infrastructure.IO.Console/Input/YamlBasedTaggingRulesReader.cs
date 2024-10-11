using FluentResults;
using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Infrastructure.IO.Console.Input.Criteria.Logbook;
using YamlDotNet.RepresentationModel;

namespace NVs.Budget.Infrastructure.IO.Console.Input;

internal class YamlBasedTaggingRulesReader : YamlReader, ITaggingRulesReader
{
    public IAsyncEnumerable<Result<TaggingRule>> ReadFrom(StreamReader reader, CancellationToken ct)
    {
        var results = ReadSync(reader);
        return results.ToAsyncEnumerable();
    }

    private IEnumerable<Result<TaggingRule>> ReadSync(StreamReader reader)
    {
        var rootResult = LoadRootNodeFrom(reader);
        if (rootResult.IsFailed)
        {
            yield return rootResult.ToResult();
            yield break;
        }

        var root = rootResult.Value;
        foreach (var (tagKey, tagRules) in root)
        {
            var tag = ReadString(tagKey, EmptyPath);
            if (!tag.IsSuccess)
            {
                yield return tag.ToResult();
                continue;
            }

            if (tagRules is not YamlSequenceNode rules)
            {
                yield return Result.Fail(new UnexpectedNodeTypeError(tagRules.GetType(), typeof(YamlSequenceNode), [tag.Value]));
                continue;
            }

            foreach (var ruleNode in rules)
            {
                var rule = ReadString(ruleNode, [tag.Value]);
                if (!rule.IsSuccess)
                {
                    yield return rule.ToResult();
                }
                else
                {
                    yield return new TaggingRule(tag.Value, rule.Value);
                }
            }
        }
    }
}
