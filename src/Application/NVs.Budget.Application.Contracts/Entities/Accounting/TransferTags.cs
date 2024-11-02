using NVs.Budget.Domain.Entities.Operations;
using NVs.Budget.Domain.ValueObjects;

namespace NVs.Budget.Application.Services.Accounting.Transfers;

public static class TransferTags
{
    public static readonly Tag Transfer = Domain.Entities.Transactions.Transfer.TransferTag;
    public static readonly Tag Source = new(nameof(Domain.Entities.Transactions.Transfer.Source));
    public static readonly Tag Sink = new(nameof(Domain.Entities.Transactions.Transfer.Sink));
    public static readonly Tag Ephemeral = new(nameof(Ephemeral));

    public static T TagSource<T>(this T operation) where T:Operation
    {
        operation.Tag(Transfer);
        operation.Tag(Source);

        return operation;
    }

    public static T TagEphemeral<T>(this T operation) where T:Operation
    {
        operation.Tag(Transfer);
        operation.Tag(Ephemeral);
        operation.Tag(Source);
        operation.Tag(Sink);

        return operation;
    }

    public static T TagSink<T>(this T operation) where T:Operation
    {
        operation.Tag(Transfer);
        operation.Tag(Sink);

        return operation;
    }

    public static T Untag<T>(this T operation) where T:Operation
    {
        operation.Untag(Transfer);
        operation.Untag(Source);
        operation.Untag(Sink);

        return operation;
    }
}
