
using NVs.Budget.Domain.Entities.Accounts;
using NVs.Budget.Domain.Entities.Transactions;

namespace NVs.Budget.Domain.Extensions;

public static class ExceptionExtensions
{
    public static Exception WithTransactionId(this Exception e, Transaction transaction) => WithData(e, "{nameof(Transaction)}.{nameof(Transaction.Id)}", transaction.Id);

    public static Exception SetTransactionId(this Exception e, Transaction transaction) => e.WithTransactionId(transaction);

    public static Exception WithAccountId(this Exception e, Account account) => WithData(e, $"{nameof(Account)}.{nameof(Transaction.Id)}", account.Id);

    public static Exception SetAccountId(this Exception e, Account account) => e.WithAccountId(account);

    public static Exception WithData(this Exception e, string key, object? value)
    {
        e.Data[key] = value;
        return e;
    }
}
