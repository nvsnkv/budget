using NMoneys;
using NVs.Budget.Application.Services.Accounting.Transfers;
using NVs.Budget.Application.Services.Storage.Accounting;

namespace NVs.Budget.Application.Entities.Accounting;

public record UnregisteredTransfer(TrackedTransaction Source, TrackedTransaction Sink, Money Fee, string Comment, DetectionAccuracy Accuracy);
