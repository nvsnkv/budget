using FluentResults;
using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Domain.Extensions;
using NVs.Budget.Infrastructure.IO.Console.Input.Criteria.Logbook;
using NVs.Budget.Utilities.Expressions;
using YamlDotNet.RepresentationModel;

namespace NVs.Budget.Infrastructure.IO.Console.Input;

internal class YamlBasedTransferCriteriaReader(ReadableExpressionsParser parser) : YamlReader, ITransferCriteriaReader
{
    private static readonly YamlScalarNode AccuracyKey = new("Accuracy");
    private static readonly YamlScalarNode CriterionKey = new("Criterion");
    public IAsyncEnumerable<Result<TransferCriterion>> ReadFrom(StreamReader reader, CancellationToken ct)
    {
        var results = ReadSync(reader);
        return results.ToAsyncEnumerable();
    }

    private IEnumerable<Result<TransferCriterion>> ReadSync(StreamReader reader)
    {
        var rootNode = LoadRootNodeFrom(reader);
        if (!rootNode.IsSuccess)
        {
            yield return rootNode.ToResult();
            yield break;
        }

        foreach (var (key, value) in rootNode.Value)
        {
            var comment = ReadString(key, EmptyPath);
            if (comment.IsFailed)
            {
                yield return comment.ToResult();
                continue;
            }

            if (value is not YamlMappingNode mapping)
            {
                yield return Result.Fail(new UnexpectedNodeTypeError(value.GetType(), typeof(YamlMappingNode), [comment.Value]));
                continue;
            }

            DetectionAccuracy accuracy;
            var accuracyVal = ReadString(mapping[AccuracyKey], [comment.Value]);
            if (accuracyVal.IsFailed)
            {
                yield return accuracyVal.ToResult();
                continue;
            }

            if (!Enum.TryParse(accuracyVal.Value, out accuracy))
            {
                yield return Result.Fail(
                    new YamlParsingError("Unexpected Accuracy value given", [comment.Value, AccuracyKey.ToString()])
                        .WithMetadata("Value", accuracyVal.Value)
                    );
            }

            var criterionVal = ReadString(mapping[CriterionKey], [comment.Value]);
            if (criterionVal.IsFailed)
            {
                yield return criterionVal.ToResult();
                continue;
            }

            var expr = parser.ParseBinaryPredicate<TrackedOperation, TrackedOperation>(criterionVal.Value);
            if (criterionVal.IsFailed)
            {
                yield return Result.Fail(new YamlParsingError("Failed to parse criterion", [comment.Value, CriterionKey.ToString()])).WithReasons(criterionVal.Reasons);
            }

            yield return new TransferCriterion(accuracy, comment.Value, expr.Value);
        }
    }
}
