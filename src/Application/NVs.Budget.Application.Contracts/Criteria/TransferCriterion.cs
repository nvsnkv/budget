using NVs.Budget.Application.Contracts.Entities.Accounting;

namespace NVs.Budget.Application.Contracts.Criteria;

public record TransferCriterion(
    DetectionAccuracy Accuracy,
    string Comment,
    Func<TrackedOperation, TrackedOperation, bool> Criterion);