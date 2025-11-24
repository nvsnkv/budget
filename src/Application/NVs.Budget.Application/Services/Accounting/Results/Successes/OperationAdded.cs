using FluentResults;
using NVs.Budget.Domain.Entities.Operations;
using NVs.Budget.Domain.Extensions;

namespace NVs.Budget.Application.Services.Accounting.Results.Successes;

internal class OperationAdded : Success
{
    public OperationAdded(Operation operation) : base("Transaction was successfully added!")
    {
        this.WithOperationId(operation);
    }
}
