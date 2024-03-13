using FluentResults;
using NVs.Budget.Domain.Entities.Transactions;
using NVs.Budget.Domain.Extensions;

namespace NVs.Budget.Application.Services.Accounting.Results.Successes;

internal class TransactionRemoved : Success
{
    public TransactionRemoved(Transaction transaction) : base("Transaction was successfully removed!")
    {
        this.WithTransactionId(transaction);
    }
}