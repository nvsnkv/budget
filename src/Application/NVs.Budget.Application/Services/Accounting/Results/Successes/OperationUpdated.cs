using FluentResults;
using NVs.Budget.Domain.Entities.Operations;
using NVs.Budget.Domain.Extensions;

namespace NVs.Budget.Application.Services.Accounting.Results.Successes;

internal class OperationUpdated : Success
{
    public OperationUpdated(Operation operation) : base("Transaction was successfully updated!")
    {
        this.WithOperationId(operation);
    }
}
