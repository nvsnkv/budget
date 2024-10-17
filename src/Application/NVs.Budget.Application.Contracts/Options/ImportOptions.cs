using NVs.Budget.Application.Contracts.Entities.Budgeting;

namespace NVs.Budget.Application.Contracts.Options;

public record ImportOptions(DetectionAccuracy? TransferConfidenceLevel);
