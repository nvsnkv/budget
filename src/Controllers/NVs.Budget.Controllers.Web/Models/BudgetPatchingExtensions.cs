using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using FluentResults;
using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Application.Contracts.Entities.Budgeting;
using NVs.Budget.Domain.Entities.Operations;
using NVs.Budget.Domain.ValueObjects;
using NVs.Budget.Domain.ValueObjects.Criteria;
using NVs.Budget.Infrastructure.Files.CSV.Contracts;
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

        private static readonly string[] RequiredFields = { nameof(UnregisteredOperation.Timestamp),nameof(UnregisteredOperation.Amount), nameof(UnregisteredOperation.Description) };

        public static Result<IReadOnlyDictionary<Regex, FileReadingSetting>> ConvertToSettings(this IDictionary<string, CsvFileReadingConfiguration> configurations)
        {
            var result = new Dictionary<Regex, FileReadingSetting>();
            foreach (var (key, value) in configurations)
            {
                var regex = new Regex(key);
                var culture = CultureInfo.InvariantCulture;
                if (!string.IsNullOrEmpty(value.CultureCode)) {
                    culture = CultureInfo.GetCultures(CultureTypes.AllCultures).FirstOrDefault(c => c.Name == value.CultureCode);
                    if (culture is null)
                    {
                        return Result.Fail(new Error($"Culture {value.CultureCode} not found"));
                    }
                }

                var encoding = Encoding.UTF8;
                if (!string.IsNullOrEmpty(value.EncodingName))
                {
                    var info = Encoding.GetEncodings().FirstOrDefault(e => e.Name == value.EncodingName);
                    if (info is null)
                    {
                        return Result.Fail(new Error($"Encoding {value.EncodingName} not found").WithMetadata("Configuration", key));
                    }

                    encoding = info.GetEncoding();
                }

                foreach (var field in RequiredFields)
                {
                    if (!value.Fields.TryGetValue(field, out var fieldValue))
                    {
                        return Result.Fail(new Error($"Field {field} is required").WithMetadata("Configuration", key).WithMetadata("Section", "Fields"));
                    }
                }

                var index = 0;
                var validation = new List<ValidationRule>();
                foreach (var rule in value.Validation) 
                {

                    if (string.IsNullOrEmpty(rule.Pattern)) {
                        return Result.Fail(new Error("Field is required").WithMetadata("Configuration", key).WithMetadata("Section", "Validation").WithMetadata("Index", index));
                    }

                    if (string.IsNullOrEmpty(rule.Value)) {
                        return Result.Fail(new Error("Value is required").WithMetadata("Configuration", key).WithMetadata("Section", "Validation").WithMetadata("Index", index));
                    }

                    if (string.IsNullOrEmpty(rule.ErrorMessage)) {
                        return Result.Fail(new Error("Error message is required").WithMetadata("Configuration", key).WithMetadata("Section", "Validation").WithMetadata("Index", index));
                    }

                    ValidationRule.ValidationCondition? condition = rule.Condition switch
                    {
                        CsvValidationCondition.Equals => ValidationRule.ValidationCondition.Equals,
                        CsvValidationCondition.NotEquals => ValidationRule.ValidationCondition.NotEquals,
                        _ => null
                    };
                    if (condition is null) {
                        return Result.Fail(new Error("Invalid condition").WithMetadata("Configuration", key).WithMetadata("Section", "Validation").WithMetadata("Index", index).WithMetadata("Condition", rule.Condition));
                    }

                    validation.Add(new ValidationRule(rule.Pattern, condition.Value, rule.Value, rule.ErrorMessage));
                    index++;
                }

                result.Add(regex, new FileReadingSetting(culture, encoding, value.Fields, value.Attributes, validation));
            }

            return result;
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
