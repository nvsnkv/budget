using FluentResults;
using NVs.Budget.Application.Contracts.Entities.Budgeting;
using NVs.Budget.Application.Contracts.Results;
using NVs.Budget.Application.Services.Accounting.Duplicates;
using NVs.Budget.Application.Services.Accounting.Results.Successes;

namespace NVs.Budget.Application.Services.Accounting.Results;

internal class UpdateResultBuilder
{
    protected readonly List<TrackedOperation> Operations = new();
    protected readonly List<TrackedTransfer> Transfers = new();
    protected readonly List<IReason> Reasons = new();

    public void Clear()
    {
        Operations.Clear();
        Transfers.Clear();
        Reasons.Clear();
    }

    public void Append(UpdateResult other)
    {
        var outdated = Operations.IntersectBy(other.Operations.Select(o => o.Id), o => o.Id).ToList();
        outdated.ForEach(o => Operations.Remove(o));
        Operations.AddRange(other.Operations);

        Transfers.AddRange(other.Transfers);
        Reasons.AddRange(other.Reasons);
    }

    public Result<TrackedOperation> Append(Result<TrackedOperation> result)
    {
        if (result.IsSuccess)
        {
            Operations.Add(result.Value);
            Reasons.Add(new OperationAdded(result.Value));
        }
        else
        {
            Reasons.AddRange(result.Reasons);
        }

        return result;
    }

    public Result<TrackedTransfer> Append(Result<TrackedTransfer> result)
    {
        if (result.IsSuccess)
        {
            Transfers.Add(result.Value);
            Reasons.Add(new TransferAdded(result.Value));
        }
        else
        {
            Reasons.AddRange(result.Reasons);
        }

        return result;
    }

    public void Append(IEnumerable<IReason> reasons)
    {
        Reasons.AddRange(reasons);
    }

    public virtual UpdateResult Build() => new(Operations, Transfers, Reasons);
}
