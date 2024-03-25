using NVs.Budget.Application.Contracts.Entities.Accounting;

namespace NVs.Budget.Application.Contracts.Options;

public record ImportOptions(bool RegisterAccounts, DetectionAccuracy? TransferConfidenceLevel);