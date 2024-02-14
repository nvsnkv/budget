﻿using NVs.Budget.Application.Services.Storage.Accounting;

namespace NVs.Budget.Application.Services.Accounting.Successes;

internal class TransferTracked : TransferAdded
{
    public TransferTracked(TrackedTransfer transfer) : base(transfer, "Transfer was successfully stored!")
    {
    }
}