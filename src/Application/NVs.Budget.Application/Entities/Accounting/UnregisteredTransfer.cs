using NMoneys;
using NVs.Budget.Application.Services.Accounting.Transfers;

namespace NVs.Budget.Application.Entities.Accounting;

public record UnregisteredTransfer(TrackedOperation Source, TrackedOperation Sink, Money Fee, string Comment, DetectionAccuracy Accuracy);
