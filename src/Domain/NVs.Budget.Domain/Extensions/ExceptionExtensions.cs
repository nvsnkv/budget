
using NVs.Budget.Domain.Entities.Accounts;
using NVs.Budget.Domain.Entities.Operations;

namespace NVs.Budget.Domain.Extensions;

public static class ExceptionExtensions
{
    public static Exception WithOperationId(this Exception e, Operation operation) => WithData(e, "{nameof(Transaction)}.{nameof(Transaction.Id)}", operation.Id);

    public static Exception SetOperationId(this Exception e, Operation operation) => e.WithOperationId(operation);

    public static Exception WithAccountId(this Exception e, Account account) => WithData(e, $"{nameof(Account)}.{nameof(Operation.Id)}", account.Id);

    public static Exception SetAccountId(this Exception e, Account account) => e.WithAccountId(account);

    public static Exception WithData(this Exception e, string key, object? value)
    {
        e.Data[key] = value;
        return e;
    }
}
