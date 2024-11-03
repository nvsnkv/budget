﻿using NVs.Budget.Domain.Entities.Operations;

namespace NVs.Budget.Domain.Extensions;

public static class ExceptionExtensions
{
    public static Exception WithOperationId(this Exception e, Operation operation) => WithData(e, "{nameof(Transaction)}.{nameof(Transaction.Id)}", operation.Id);

    public static Exception SetOperationId(this Exception e, Operation operation) => e.WithOperationId(operation);

    public static Exception WithAccountId(this Exception e, Entities.Accounts.Budget budget) => WithData(e, $"{nameof(Entities.Accounts.Budget)}.{nameof(Operation.Id)}", budget.Id);

    public static Exception SetAccountId(this Exception e, Entities.Accounts.Budget budget) => e.WithAccountId(budget);

    public static Exception WithData(this Exception e, string key, object? value)
    {
        e.Data[key] = value;
        return e;
    }
}
