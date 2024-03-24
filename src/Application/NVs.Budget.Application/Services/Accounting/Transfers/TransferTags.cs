using NVs.Budget.Domain.Entities.Operations;
using NVs.Budget.Domain.ValueObjects;

namespace NVs.Budget.Application.Services.Accounting.Transfers;

public static class TransferTags
{
    public static readonly Tag Transfer = Domain.Entities.Operations.Transfer.TransferTag;
    public static readonly Tag Source = new(nameof(Domain.Entities.Operations.Transfer.Source));
    public static readonly Tag Sink = new(nameof(Domain.Entities.Operations.Transfer.Sink));

    internal static void TagSource(this Operation operation)
    {
        operation.Tag(Transfer);
        operation.Tag(Source);
    }

    internal static void TagSink(this Operation operation)
    {
        operation.Tag(Transfer);
        operation.Tag(Sink);
    }
}
