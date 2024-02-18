using NVs.Budget.Domain.Entities.Transactions;
using NVs.Budget.Domain.ValueObjects;

namespace NVs.Budget.Application.Services.Accounting.Transfers;

public static class TransferTags
{
    public static readonly Tag Transfer = Domain.Entities.Transactions.Transfer.TransferTag;
    public static readonly Tag Source = new(nameof(Domain.Entities.Transactions.Transfer.Source));
    public static readonly Tag Sink = new(nameof(Domain.Entities.Transactions.Transfer.Sink));

    internal static void TagSource(this Transaction transaction)
    {
        transaction.Tag(Transfer);
        transaction.Tag(Source);
    }

    internal static void TagSink(this Transaction transaction)
    {
        transaction.Tag(Transfer);
        transaction.Tag(Sink);
    }
}
