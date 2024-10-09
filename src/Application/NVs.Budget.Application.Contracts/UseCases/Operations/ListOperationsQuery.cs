using MediatR;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Application.Contracts.Queries;

namespace NVs.Budget.Application.Contracts.UseCases.Operations;

public record ListOperationsQuery(OperationQuery Query) : IStreamRequest<TrackedOperation>;
