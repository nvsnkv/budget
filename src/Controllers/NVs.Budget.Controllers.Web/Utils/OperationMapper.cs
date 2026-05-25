using FluentResults;
using NMoneys;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Controllers.Web.Models;
using NVs.Budget.Utilities.Utc;
using NVs.Budget.Domain.Entities.Operations;
using NVs.Budget.Domain.ValueObjects;

namespace NVs.Budget.Controllers.Web.Utils;

public class OperationMapper(MoneyMapper moneyMapper)
{
    public OperationResponse ToResponse(TrackedOperation operation)
    {
        return new OperationResponse(
            operation.Id,
            operation.Version ?? string.Empty,
            operation.Timestamp.AsUtcFromApi(),
            new MoneyResponse(operation.Amount.Amount, operation.Amount.CurrencyCode.ToString()),
            operation.Description,
            operation.Notes,
            operation.Budget.Id,
            operation.Tags.Select(t => t.Value).ToList(),
            operation.Attributes.Count > 0 ? new Dictionary<string, object>(operation.Attributes) : null
        );
    }

    public OperationResponse ToResponse(Operation operation)
    {
        // If it's a TrackedOperation, use the TrackedOperation overload
        if (operation is TrackedOperation trackedOperation)
        {
            return ToResponse(trackedOperation);
        }

        // Otherwise, map as base Operation
        return new OperationResponse(
            operation.Id,
            string.Empty, // Operation doesn't have Version
            operation.Timestamp.AsUtcFromApi(),
            new MoneyResponse(operation.Amount.Amount, operation.Amount.CurrencyCode.ToString()),
            operation.Description,
            operation.Notes,
            operation.Budget.Id,
            operation.Tags.Select(t => t.Value).ToList(),
            operation.Attributes.Count > 0 ? new Dictionary<string, object>(operation.Attributes) : null
        );
    }

    public Result<UnregisteredOperation> FromRequest(UnregisteredOperationRequest request, string clientIanaTimeZoneId)
    {
        var moneyResult = moneyMapper.ParseMoney(request.Amount);
        if (moneyResult.IsFailed)
        {
            return Result.Fail<UnregisteredOperation>(moneyResult.Errors);
        }

        var timestampResult = ParseManualImportTimestamp(request.Timestamp, clientIanaTimeZoneId);
        if (timestampResult.IsFailed)
        {
            return Result.Fail<UnregisteredOperation>(timestampResult.Errors);
        }

        var attributes = request.Attributes != null 
            ? new Dictionary<string, object>(request.Attributes) 
            : null;

        return Result.Ok(new UnregisteredOperation(
            timestampResult.Value,
            moneyResult.Value,
            request.Description,
            attributes
        ));
    }

    private Result<DateTime> ParseManualImportTimestamp(string timestamp, string clientIanaTimeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timestamp))
        {
            return Result.Fail<DateTime>("Timestamp is required.");
        }

        if (DateTimeOffset.TryParse(timestamp, out var withOffset))
        {
            return Result.Ok(withOffset.UtcDateTime.AsUtcFromApi());
        }

        if (!DateTime.TryParse(timestamp, out var localWallClock))
        {
            return Result.Fail<DateTime>($"Invalid timestamp format: {timestamp}");
        }

        try
        {
            return Result.Ok(localWallClock.AsUtcFromImport(DateTimeKind.Local, clientIanaTimeZoneId));
        }
        catch (ArgumentException ex)
        {
            return Result.Fail<DateTime>(ex.Message);
        }
    }

    public Result<TrackedOperation> FromRequest(UpdateOperationRequest request, TrackedBudget budget)
    {
        var moneyResult = moneyMapper.ParseMoney(request.Amount);
        if (moneyResult.IsFailed)
        {
            return Result.Fail<TrackedOperation>(moneyResult.Errors);
        }

        var tags = request.Tags.Select(t => new Tag(t)).ToList();
        var attributes = request.Attributes != null 
            ? new Dictionary<string, object>(request.Attributes) 
            : null;

        var operation = new TrackedOperation(
            request.Id,
            request.Timestamp.AsUtcFromApi(),
            moneyResult.Value,
            request.Description,
            request.Notes ?? string.Empty,
            budget,
            tags,
            attributes
        )
        {
            Version = request.Version
        };

        return Result.Ok(operation);
    }

    public Result<DetectionAccuracy> ParseDetectionAccuracy(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return Result.Ok<DetectionAccuracy>(default);
        }

        if (!Enum.TryParse<DetectionAccuracy>(value, true, out var accuracy))
        {
            return Result.Fail<DetectionAccuracy>($"Invalid DetectionAccuracy value: {value}");
        }

        return Result.Ok(accuracy);
    }
}

