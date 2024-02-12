using FluentResults;
using NVs.Budget.Domain.Entities.Transactions;

namespace NVs.Budget.Domain.Extensions;

public static class ReasonExtensions
{
    public static T WithMetadata<T>(this T reason, string key, object? value) where T: IReason
    {
        reason.Metadata[key] = value;
        return reason;
    }

    public static T WithTransactionId<T>(this T reason, Transaction t) where T: IReason
    {
        return reason.WithMetadata($"{nameof(Transaction)}.{nameof(Transaction.Id)}", t.Id);
    }
}
