using FluentResults;
using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Application.Contracts.Entities.Budgeting;
using NVs.Budget.Domain.Entities.Operations;
using NVs.Budget.Domain.ValueObjects;
using NVs.Budget.Domain.ValueObjects.Criteria;
using NVs.Budget.Utilities.Expressions;

namespace NVs.Budget.Controllers.Web.Models
{
    public static class BudgetPatchingExtensions
    {
        public static Result<TrackedBudget> Patch(this TrackedBudget budget, BudgetConfiguration patch, ReadableExpressionsParser parser)
        {
            var tags = ParseTaggingCriteria(patch.Tags, parser).ToList();
            if (tags.Any(t => t.IsFailed))
            {
                return Result.Fail(tags.SelectMany(t => t.Errors).ToList());
            }

            var transfers = ParseTransferCriteria(patch.Transfers, parser).ToList();
            if (transfers.Any(t => t.IsFailed))
            {
                return Result.Fail(transfers.SelectMany(t => t.Errors).ToList());
            }

            var logbook = ParseLogbookCriteria(patch.Logbook, parser);
            if (logbook.IsFailed)
            {
                return Result.Fail(logbook.Errors);
            }

            return new TrackedBudget(
                budget.Id,
                patch.Name,
                budget.Owners,
                tags.Select(t => t.Value),
                transfers.Select(t => t.Value),
                logbook.Value
            )
            {
                Version = patch.Version
            };
        }

        private static IEnumerable<Result<TransferCriterion>> ParseTransferCriteria(IDictionary<string, IEnumerable<TransferCriterionExpression>>? transfers, ReadableExpressionsParser parser)
        {
            foreach (var (comment, criterions) in transfers ?? Enumerable.Empty<KeyValuePair<string, IEnumerable<TransferCriterionExpression>>>())
            {
                foreach (var criterion in criterions)
                {
                    foreach (var condition in criterion.Criteria ?? Enumerable.Empty<string>())
                    {
                        var conditionExpression = parser.ParseBinaryPredicate<TrackedOperation, TrackedOperation>(condition);
                        if (conditionExpression.IsFailed)
                        {
                            yield return conditionExpression.ToResult<TransferCriterion>();
                        }

                        yield return new TransferCriterion(criterion.Accuracy, comment, conditionExpression.Value);
                    }
                }
            }
        }

        private static IEnumerable<Result<TaggingCriterion>> ParseTaggingCriteria(IDictionary<string, IEnumerable<string>>? tags, ReadableExpressionsParser parser)
        {
            foreach (var (tag, conditiosns) in tags ?? Enumerable.Empty<KeyValuePair<string, IEnumerable<string>>>())
            {
                var tagExpression = parser.ParseUnaryConversion<TrackedOperation>(tag);
                if (tagExpression.IsFailed)
                {
                    yield return tagExpression.ToResult<TaggingCriterion>();
                }
                foreach (var condition in conditiosns)
                {
                    var conditionExpression = parser.ParseUnaryPredicate<TrackedOperation>(condition);
                    if (conditionExpression.IsFailed)
                    {
                        yield return conditionExpression.ToResult<TaggingCriterion>();
                    }

                    yield return new TaggingCriterion(tagExpression.Value, conditionExpression.Value);
                }
            }
        }

        private static Result<LogbookCriteria> ParseLogbookCriteria(IDictionary<string, LogbookCriteriaExpression>? logbook, ReadableExpressionsParser parser)
        {
            if (logbook is null)
            {
                return Result.Ok(LogbookCriteria.Universal);
            }

            var subcriteria = new List<LogbookCriteria>();

            foreach (var (description, criterions) in logbook)
            {
                var parsedCriteria = ParseLogbookCriteria(description, criterions, parser);
                if (parsedCriteria.IsFailed)
                {
                    return Result.Fail(new Error($"Failed to parse logbook criteria for {description}").CausedBy(parsedCriteria.Errors));
                }

                subcriteria.Add(parsedCriteria.Value);
            }

            return Result.Ok(new LogbookCriteria(string.Empty, subcriteria.AsReadOnly(), null, null, null, null, true));
        }

        private static Result<LogbookCriteria> ParseLogbookCriteria(string description, LogbookCriteriaExpression criterions, ReadableExpressionsParser parser)
        {
            var subcriteria = new List<LogbookCriteria>();
            if (criterions.Subcriteria is not null)
                foreach (var (subDescr, subCriterion) in criterions.Subcriteria)
                {
                    var parsedCriteria = ParseLogbookCriteria(subDescr, subCriterion, parser);
                    if (parsedCriteria.IsFailed)
                    {
                        return parsedCriteria;
                    }

                    subcriteria.Add(parsedCriteria.Value);
                }

            return criterions switch
            {
                { Criteria: not null } => parser.ParseUnaryPredicate<Operation>(criterions.Criteria).Map(c => new LogbookCriteria(description, subcriteria.AsReadOnly(), null, null, null, c, false)),
                { Substitution: not null } => parser.ParseUnaryConversion<Operation>(criterions.Substitution).Map(s => new LogbookCriteria(description, subcriteria.AsReadOnly(), null, null, s, null, false)),
                { Tags: not null } => new LogbookCriteria(description, subcriteria.AsReadOnly(), criterions.Type ?? TagBasedCriterionType.OneOf, criterions.Tags.Select(t => new Tag(t)).ToList().AsReadOnly(), null, null, false),
                _ => LogbookCriteria.Universal
            };
        }
    }
}