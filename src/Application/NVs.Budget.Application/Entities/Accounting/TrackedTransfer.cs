﻿using NMoneys;
using NVs.Budget.Application.Services.Accounting.Transfers;
using NVs.Budget.Domain.Entities.Operations;

namespace NVs.Budget.Application.Entities.Accounting;

public class TrackedTransfer : Transfer
{
    public TrackedTransfer(Operation source, Operation sink, string comment) : base(source, sink, comment)
    {
    }

    public TrackedTransfer(Operation source, Operation sink, Money fee, string comment) : base(source, sink, fee, comment)
    {
    }

    public DetectionAccuracy Accuracy { get; init; }
}
