using NVs.Budget.Application.Contracts.Entities.Accounting;

namespace NVs.Budget.Application.Services.Accounting.Results.Successes;

internal class TransferTracked(TrackedTransfer transfer) : TransferAdded(transfer, "Transfer was successfully stored!");
