using FluentResults;
using NVs.Budget.Domain.Entities.Transactions;

namespace NVs.Budget.Domain.Extensions;

public static class ErrorExtensions
{
    public static IError WithMetadata(this IError error, string key, object? value)
    {
        error.Metadata[key] = value;
        return error;
    }

    public static IError WithTransactionId(this IError error, Transaction t)
    {
        return error.WithMetadata($"{nameof(Transaction)}.{nameof(Transaction.Id)}", t.Id);
    }
}
