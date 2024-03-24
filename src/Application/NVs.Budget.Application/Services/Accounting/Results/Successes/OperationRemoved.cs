using FluentResults;
using NVs.Budget.Domain.Entities.Operations;
using NVs.Budget.Domain.Extensions;

namespace NVs.Budget.Application.Services.Accounting.Results.Successes;

internal class OperationRemoved : Success
{
    public OperationRemoved(Operation operation) : base("Transaction was successfully removed!")
    {
        this.WithTransactionId(operation);
    }
}
