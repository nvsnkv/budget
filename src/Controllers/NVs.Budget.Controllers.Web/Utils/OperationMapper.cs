using FluentResults;
using NMoneys;
using NVs.Budget.Application.Contracts.Entities.Budgeting;
using NVs.Budget.Controllers.Web.Models;
using NVs.Budget.Domain.ValueObjects;

namespace NVs.Budget.Controllers.Web.Utils;

public class OperationMapper(MoneyMapper moneyMapper)
{
    public OperationResponse ToResponse(TrackedOperation operation)
    {
        return new OperationResponse(
            operation.Id,
            operation.Version ?? string.Empty,
            operation.Timestamp,
            new MoneyResponse(operation.Amount.Amount, operation.Amount.CurrencyCode.ToString()),
            operation.Description,
            operation.Budget.Id,
            operation.Tags.Select(t => t.Value).ToList(),
            operation.Attributes.Count > 0 ? new Dictionary<string, object>(operation.Attributes) : null
        );
    }

    public Result<UnregisteredOperation> FromRequest(UnregisteredOperationRequest request)
    {
        var moneyResult = moneyMapper.ParseMoney(request.Amount);
        if (moneyResult.IsFailed)
        {
            return Result.Fail<UnregisteredOperation>(moneyResult.Errors);
        }

        var attributes = request.Attributes != null 
            ? new Dictionary<string, object>(request.Attributes) 
            : null;

        return Result.Ok(new UnregisteredOperation(
            request.Timestamp,
            moneyResult.Value,
            request.Description,
            attributes
        ));
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
            request.Timestamp,
            moneyResult.Value,
            request.Description,
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

