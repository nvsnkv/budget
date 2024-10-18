using NVs.Budget.Application.Contracts.Entities.Budgeting;

namespace NVs.Budget.Application.Services.Accounting.Results.Successes;

internal class TransferTracked(TrackedTransfer transfer) : TransferAdded(transfer, "Transfer was successfully stored!");
