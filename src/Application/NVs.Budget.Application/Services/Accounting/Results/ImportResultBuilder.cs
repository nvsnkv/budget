using FluentResults;
using NVs.Budget.Application.Contracts.Entities.Budgeting;
using NVs.Budget.Application.Contracts.Results;
using NVs.Budget.Application.Services.Accounting.Duplicates;
using NVs.Budget.Application.Services.Accounting.Results.Successes;

namespace NVs.Budget.Application.Services.Accounting.Results;

internal class ImportResultBuilder(DuplicatesDetector detector) : UpdateResultBuilder
{
    public override ImportResult Build()
    {
        var duplicates = detector.DetectDuplicates(Operations);
        return new ImportResult(Operations, Transfers, duplicates, Reasons);
    }
}
