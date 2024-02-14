using NVs.Budget.Application.Services.Storage.Accounting;

namespace NVs.Budget.Application.Services.Accounting;

public record TransferCriteria(
    DetectionAccuracy Accuracy,
    string Comment,
    Func<TrackedTransaction, TrackedTransaction, bool> Criterion);