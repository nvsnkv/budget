using FluentResults;
using NMoneys;
using NVs.Budget.Application.Services.Accounting;
using NVs.Budget.Domain.Entities.Transactions;

namespace NVs.Budget.Application.Services.Storage.Accounting;

public interface ITransfersRepository
{
    Task<TrackedTransfer?> GetTransfer(Transaction source, Transaction sink, CancellationToken ct);
    Task<Result> Track(TrackedTransfer transfer, CancellationToken ct);
}

public class TrackedTransfer : Transfer
{
    public TrackedTransfer(Transaction source, Transaction sink, string comment) : base(source, sink, comment)
    {
    }

    public TrackedTransfer(Transaction source, Transaction sink, Money fee, string comment) : base(source, sink, fee, comment)
    {
    }

    public DetectionAccuracy Accuracy { get; init; }
}
