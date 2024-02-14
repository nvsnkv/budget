using FluentResults;
using NVs.Budget.Domain.Entities.Transactions;
using NVs.Budget.Domain.Extensions;

namespace NVs.Budget.Application.Services.Accounting.Successes;

internal class TransactionAdded : Success
{
    public TransactionAdded(Transaction transaction) : base("Transaction was successfully added!")
    {
        this.WithTransactionId(transaction);
    }
}
