using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Utilities.Expressions;

namespace NVs.Budget.Application.Contracts.Criteria;

public record TransferCriterion(
    DetectionAccuracy Accuracy,
    string Comment,
    ReadableExpression<Func<TrackedOperation, TrackedOperation, bool>> Criterion);
