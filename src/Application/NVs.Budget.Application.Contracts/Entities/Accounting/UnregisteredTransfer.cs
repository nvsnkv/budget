using NMoneys;

namespace NVs.Budget.Application.Contracts.Entities.Budgeting;

public record UnregisteredTransfer(TrackedOperation Source, TrackedOperation Sink, Money Fee, string Comment, DetectionAccuracy Accuracy);
