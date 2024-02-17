using FluentResults;
using NVs.Budget.Domain.Entities.Transactions;
using NVs.Budget.Domain.Extensions;

namespace NVs.Budget.Application.Services.Accounting.Results.Successes;

internal class TransactionUpdated : Success
{
    public TransactionUpdated(Transaction transaction) : base("Transaction was successfully updated!")
    {
        this.WithTransactionId(transaction);
    }
}