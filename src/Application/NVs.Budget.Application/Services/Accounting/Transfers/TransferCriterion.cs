using NVs.Budget.Application.Entities.Accounting;

namespace NVs.Budget.Application.Services.Accounting.Transfers;

public record TransferCriterion(
    DetectionAccuracy Accuracy,
    string Comment,
    Func<TrackedTransaction, TrackedTransaction, bool> Criterion);