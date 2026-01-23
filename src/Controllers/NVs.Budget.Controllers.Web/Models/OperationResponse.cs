using FluentResults;

namespace NVs.Budget.Controllers.Web.Models;

public record OperationResponse(
    Guid Id,
    string Version,
    DateTime Timestamp,
    MoneyResponse Amount,
    string Description,
    string Notes,
    Guid BudgetId,
    IReadOnlyCollection<string> Tags,
    Dictionary<string, object>? Attributes
);

public record MoneyResponse(
    decimal Value,
    string CurrencyCode
);

public record UnregisteredOperationRequest(
    DateTime Timestamp,
    MoneyResponse Amount,
    string Description,
    string? Notes,
    Dictionary<string, object>? Attributes
);

public record UpdateOperationRequest(
    Guid Id,
    string Version,
    DateTime Timestamp,
    MoneyResponse Amount,
    string Description,
    string? Notes,
    IReadOnlyCollection<string> Tags,
    Dictionary<string, object>? Attributes
);

public record UpdateOperationsRequest(
    string BudgetVersion,
    IReadOnlyCollection<UpdateOperationRequest> Operations,
    string? TransferConfidenceLevel,
    string TaggingMode
);

public record RemoveOperationsRequest(
    string Criteria
);

public record RetagOperationsRequest(
    string BudgetVersion,
    string Criteria,
    bool FromScratch
);

// Result response models
public record ImportResultResponse(
    IReadOnlyCollection<OperationResponse> RegisteredOperations,
    IReadOnlyCollection<IReadOnlyCollection<OperationResponse>> Duplicates,
    IReadOnlyCollection<IError> Errors,
    IReadOnlyCollection<ISuccess> Successes);

public record UpdateResultResponse(
    IReadOnlyCollection<OperationResponse> UpdatedOperations,
    IReadOnlyCollection<IError> Errors,
    IReadOnlyCollection<ISuccess> Successes
);

public record DeleteResultResponse(
    IReadOnlyCollection<IError> Errors,
    IReadOnlyCollection<ISuccess> Successes
);

public record RetagResultResponse(
    IReadOnlyCollection<IError> Errors,
    IReadOnlyCollection<ISuccess> Successes
);

// Logbook models
public record NamedRangeResponse(
    string Name,
    DateTime From,
    DateTime Till
);

public record LogbookEntryResponse(
    string Description,
    MoneyResponse Sum,
    DateTime From,
    DateTime Till,
    int OperationsCount,
    IReadOnlyCollection<OperationResponse> Operations,
    IReadOnlyCollection<LogbookEntryResponse> Children
);

public record RangedLogbookEntryResponse(
    NamedRangeResponse Range,
    LogbookEntryResponse Entry
);

public record LogbookResponse(
    IReadOnlyCollection<RangedLogbookEntryResponse> Ranges,
    IReadOnlyCollection<IError> Errors,
    IReadOnlyCollection<ISuccess> Successes
);

