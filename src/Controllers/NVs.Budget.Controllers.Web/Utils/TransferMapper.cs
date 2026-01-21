using FluentResults;
using NMoneys;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Controllers.Web.Models;
using NVs.Budget.Domain.Entities.Transactions;

namespace NVs.Budget.Controllers.Web.Utils;

public class TransferMapper(OperationMapper operationMapper, MoneyMapper moneyMapper)
{
    public TransferResponse ToResponse(TrackedTransfer transfer)
    {
        return new TransferResponse(
            transfer.Source.Id,
            operationMapper.ToResponse(transfer.Source), // Will check if it's TrackedOperation internally
            transfer.Sink.Id,
            operationMapper.ToResponse(transfer.Sink), // Will check if it's TrackedOperation internally
            new MoneyResponse(transfer.Fee.Amount, transfer.Fee.CurrencyCode.ToString()),
            transfer.Comment,
            transfer.Accuracy.ToString()
        );
    }

    public TransferResponse ToResponse(Transfer transfer, string accuracy = "Unknown")
    {
        // If it's a TrackedTransfer, use the TrackedTransfer overload
        if (transfer is TrackedTransfer trackedTransfer)
        {
            return ToResponse(trackedTransfer);
        }

        // Otherwise, map as base Transfer
        return new TransferResponse(
            transfer.Source.Id,
            operationMapper.ToResponse(transfer.Source),
            transfer.Sink.Id,
            operationMapper.ToResponse(transfer.Sink),
            new MoneyResponse(transfer.Fee.Amount, transfer.Fee.CurrencyCode.ToString()),
            transfer.Comment,
            accuracy
        );
    }

    public Result<UnregisteredTransfer> FromRequest(
        RegisterTransferRequest request,
        TrackedOperation source,
        TrackedOperation sink)
    {
        // Parse fee if provided
        Money fee;
        if (request.Fee != null)
        {
            var feeResult = moneyMapper.ParseMoney(request.Fee);
            if (feeResult.IsFailed)
            {
                return Result.Fail<UnregisteredTransfer>(feeResult.Errors);
            }
            fee = feeResult.Value;
        }
        else
        {
            // Calculate fee as difference
            fee = sink.Amount + source.Amount;
        }

        // Parse accuracy
        var accuracyResult = operationMapper.ParseDetectionAccuracy(request.Accuracy);
        if (accuracyResult.IsFailed)
        {
            return Result.Fail<UnregisteredTransfer>(accuracyResult.Errors);
        }

        var transfer = new UnregisteredTransfer(
            source,
            sink,
            fee,
            request.Comment,
            accuracyResult.Value
        );

        return Result.Ok(transfer);
    }

    public TransferResponse ToResponse(UnregisteredTransfer transfer)
    {
        return new TransferResponse(
            transfer.Source.Id,
            operationMapper.ToResponse(transfer.Source),
            transfer.Sink.Id,
            operationMapper.ToResponse(transfer.Sink),
            new MoneyResponse(transfer.Fee.Amount, transfer.Fee.CurrencyCode.ToString()),
            transfer.Comment,
            transfer.Accuracy.ToString()
        );
    }

    public Result<DetectionAccuracy> ParseDetectionAccuracy(string? value)
    {
        return operationMapper.ParseDetectionAccuracy(value);
    }
}

