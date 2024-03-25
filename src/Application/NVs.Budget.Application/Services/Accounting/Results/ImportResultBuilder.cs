using FluentResults;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Application.Contracts.Results;
using NVs.Budget.Application.Services.Accounting.Duplicates;
using NVs.Budget.Application.Services.Accounting.Results.Successes;

namespace NVs.Budget.Application.Services.Accounting.Results;

internal class ImportResultBuilder(DuplicatesDetector detector)
{
    private readonly List<TrackedOperation> _transactions = new();
    private readonly List<TrackedTransfer> _transfers = new();
    private readonly List<IReason> _reasons = new();

    public void Clear()
    {
        _transactions.Clear();
        _transfers.Clear();
        _reasons.Clear();
    }

    public void Append(Result<TrackedOperation> result)
    {
        if (result.IsSuccess)
        {
            _transactions.Add(result.Value);
            _reasons.Add(new OperationAdded(result.Value));
        }
        else
        {
            _reasons.AddRange(result.Reasons);
        }
    }

    public void Append(Result<TrackedTransfer> result)
    {
        if (result.IsSuccess)
        {
            _transfers.Add(result.Value);
            _reasons.Add(new TransferAdded(result.Value));
        }
        else
        {
            _reasons.AddRange(result.Reasons);
        }
    }

    public ImportResult Build()
    {
        var duplicates = detector.DetectDuplicates(_transactions);
        return new ImportResult(_transactions, _transfers, duplicates, _reasons);
    }

    public void Append(Result<TrackedAccount> result)
    {
        if (result.IsSuccess)
        {
            _reasons.Add(new AccountAdded(result.Value));
        }
        else
        {
            _reasons.AddRange(result.Reasons);
        }
    }
}
