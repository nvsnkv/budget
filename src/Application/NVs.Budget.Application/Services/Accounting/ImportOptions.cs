namespace NVs.Budget.Application.Services.Accounting;

public record ImportOptions(bool RegisterAccounts, DetectionAccuracy? TransferConfidenceLevel);