﻿using FluentResults;
using NVs.Budget.Domain.Entities.Accounts;
using NVs.Budget.Domain.Entities.Operations;

namespace NVs.Budget.Domain.Extensions;

public static class ReasonExtensions
{
    public static T WithMetadata<T>(this T reason, string key, object? value) where T: IReason
    {
        reason.Metadata[key] = value;
        return reason;
    }

    public static T WithTransactionId<T>(this T reason, Operation t) where T: IReason
    {
        return reason.WithMetadata($"{nameof(Operation)}.{nameof(Operation.Id)}", t.Id);
    }

    public static T WithAccountId<T>(this T reason, Account a) where T : IReason
    {
        return reason.WithMetadata($"{nameof(Account)}.{nameof(Account.Id)}", a.Id);
    }

    public static Result<T> WithReason<T>(this Result<T> result, IReason reason)
    {
        result.Reasons.Add(reason);
        return result;
    }
}
