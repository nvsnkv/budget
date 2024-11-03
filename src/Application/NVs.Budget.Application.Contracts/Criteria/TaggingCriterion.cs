using NVs.Budget.Application.Contracts.Entities.Budgeting;
using NVs.Budget.Utilities.Expressions;

namespace NVs.Budget.Application.Contracts.Criteria;

public record TaggingCriterion(
    ReadableExpression<Func<TrackedOperation, string>> Tag,
    ReadableExpression<Func<TrackedOperation, bool>> Condition
);
