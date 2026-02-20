using FluentResults;
using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Controllers.Web.Models;
using NVs.Budget.Domain.Entities.Operations;
using NVs.Budget.Utilities.Expressions;

namespace NVs.Budget.Controllers.Web.Utils;

public class BudgetMapper(ReadableExpressionsParser parser)
{
    public BudgetResponse ToResponse(TrackedBudget budget)
    {
        return new BudgetResponse(
            budget.Id,
            budget.Name,
            budget.Version ?? string.Empty,
            budget.Owners,
            budget.TaggingCriteria.Select(ToResponse).ToList(),
            budget.TransferCriteria.Select(ToResponse).ToList(),
            budget.LogbookCriteria.Select(ToResponse).ToList()
        );
    }

    private TaggingCriterionResponse ToResponse(TaggingCriterion criterion)
    {
        return new TaggingCriterionResponse
        {
            Tag = criterion.Tag.ToString(),
            Condition = criterion.Condition.ToString()
        };
    }

    private TransferCriterionResponse ToResponse(TransferCriterion criterion)
    {
        return new TransferCriterionResponse
        {
            Accuracy = criterion.Accuracy.ToString(),
            Comment = criterion.Comment,
            Criterion = criterion.Criterion.ToString()
        };
    }

    private LogbookCriteriaResponse ToResponse(LogbookCriteria criteria)
    {
        return new LogbookCriteriaResponse
        {
            Description = criteria.Description,
            Subcriteria = criteria.Subcriteria?.Select(ToResponse).ToList(),
            Type = criteria.Type?.ToString(),
            Tags = criteria.Tags?.Select(t => t.Value).ToList(),
            Substitution = criteria.Substitution?.ToString(),
            Criteria = criteria.Criteria?.ToString(),
            IsUniversal = criteria.IsUniversal
        };
    }

    public Result<TaggingCriterion> FromRequest(TaggingCriterionResponse request)
    {
        var tagResult = parser.ParseUnaryConversion<TrackedOperation>(request.Tag);
        if (tagResult.IsFailed)
        {
            return Result.Fail<TaggingCriterion>(tagResult.Errors);
        }

        var conditionResult = parser.ParseUnaryPredicate<TrackedOperation>(request.Condition);
        if (conditionResult.IsFailed)
        {
            return Result.Fail<TaggingCriterion>(conditionResult.Errors);
        }

        return Result.Ok(new TaggingCriterion(tagResult.Value, conditionResult.Value));
    }

    public Result<TransferCriterion> FromRequest(TransferCriterionResponse request)
    {
        if (!Enum.TryParse<DetectionAccuracy>(request.Accuracy, out var accuracy))
        {
            return Result.Fail<TransferCriterion>($"Invalid DetectionAccuracy value: {request.Accuracy}");
        }

        var criterionResult = parser.ParseBinaryPredicate<TrackedOperation, TrackedOperation>(request.Criterion);
        if (criterionResult.IsFailed)
        {
            return Result.Fail<TransferCriterion>(criterionResult.Errors);
        }

        return Result.Ok(new TransferCriterion(accuracy, request.Comment, criterionResult.Value));
    }

    public Result<LogbookCriteria> FromRequest(LogbookCriteriaResponse request)
    {
        // Parse subcriteria recursively if present
        List<LogbookCriteria>? subcriteria = null;
        if (request.Subcriteria != null)
        {
            var duplicateNames = request.Subcriteria
                .GroupBy(c => c.Description.Trim(), StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateNames.Count != 0)
            {
                return Result.Fail<LogbookCriteria>(
                    $"Duplicate LogbookCriteria descriptions are not allowed: {string.Join(", ", duplicateNames)}");
            }

            subcriteria = new List<LogbookCriteria>();
            foreach (var sub in request.Subcriteria)
            {
                var subResult = FromRequest(sub);
                if (subResult.IsFailed)
                {
                    return Result.Fail<LogbookCriteria>(subResult.Errors);
                }
                subcriteria.Add(subResult.Value);
            }
        }

        // Parse Type if present
        Domain.ValueObjects.Criteria.TagBasedCriterionType? type = null;
        if (request.Type != null)
        {
            if (!Enum.TryParse<Domain.ValueObjects.Criteria.TagBasedCriterionType>(request.Type, out var parsedType))
            {
                return Result.Fail<LogbookCriteria>($"Invalid TagBasedCriterionType value: {request.Type}");
            }
            type = parsedType;
        }

        // Parse Tags if present
        Domain.ValueObjects.Tag[]? tags = null;
        if (request.Tags != null)
        {
            tags = request.Tags.Select(t => new Domain.ValueObjects.Tag(t)).ToArray();
        }

        // Parse Substitution if present
        ReadableExpression<Func<Operation, string>>? substitution = null;
        if (request.Substitution != null)
        {
            var substitutionResult = parser.ParseUnaryConversion<Operation>(request.Substitution);
            if (substitutionResult.IsFailed)
            {
                return Result.Fail<LogbookCriteria>(substitutionResult.Errors);
            }
            substitution = substitutionResult.Value;
        }

        // Parse Criteria if present
        ReadableExpression<Func<Operation, bool>>? criteria = null;
        if (request.Criteria != null)
        {
            var criteriaResult = parser.ParseUnaryPredicate<Operation>(request.Criteria);
            if (criteriaResult.IsFailed)
            {
                return Result.Fail<LogbookCriteria>(criteriaResult.Errors);
            }
            criteria = criteriaResult.Value;
        }

        return Result.Ok(new LogbookCriteria(
            request.Description,
            subcriteria,
            type,
            tags,
            substitution,
            criteria,
            request.IsUniversal
        ));
    }

    public Result<IReadOnlyCollection<LogbookCriteria>> FromRequest(IReadOnlyCollection<LogbookCriteriaResponse> requests)
    {
        var parsed = new List<LogbookCriteria>();
        foreach (var request in requests)
        {
            var parseResult = FromRequest(request);
            if (parseResult.IsFailed)
            {
                return Result.Fail<IReadOnlyCollection<LogbookCriteria>>(parseResult.Errors);
            }

            parsed.Add(parseResult.Value);
        }

        var duplicateNames = parsed
            .GroupBy(c => c.Description.Trim(), StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicateNames.Count != 0)
        {
            return Result.Fail<IReadOnlyCollection<LogbookCriteria>>(
                $"Duplicate LogbookCriteria descriptions are not allowed: {string.Join(", ", duplicateNames)}");
        }

        return Result.Ok<IReadOnlyCollection<LogbookCriteria>>(parsed);
    }
}
