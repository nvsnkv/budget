using System.Diagnostics.CodeAnalysis;
using NVs.Budget.Domain.Entities.Operations;
using NVs.Budget.Domain.ValueObjects;

namespace NVs.Budget.Application.Contracts.Entities.Accounting;

public static class TransferTags
{
    public static readonly Tag Transfer = Domain.Entities.Transactions.Transfer.TransferTag;
    public static readonly Tag Source = new(nameof(Domain.Entities.Transactions.Transfer.Source));
    public static readonly Tag Sink = new(nameof(Domain.Entities.Transactions.Transfer.Sink));
    public static readonly Tag Ephemeral = new(nameof(Ephemeral));

    private static Tag[]? _tags;
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "it's a static class, all private fields looks better with underscore")]
    private static readonly object _tagsLock = new ();
    public static Tag[] All
    {
        get
        {
            if (_tags is null)
            {
                lock (_tagsLock)
                {
                    _tags ??= [Transfer, Source, Sink, Ephemeral];
                }
            }

            return _tags;
        }
    }

    public static T TagSource<T>(this T operation) where T:Operation
    {
        operation.Tag(Transfer);
        operation.Tag(Source);

        return operation;
    }

    public static T TagEphemeral<T>(this T operation) where T:Operation
    {
        operation.Tag(Ephemeral);

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
