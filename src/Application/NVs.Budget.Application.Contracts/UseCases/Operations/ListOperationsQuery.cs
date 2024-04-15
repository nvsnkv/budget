using MediatR;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Application.Contracts.Queries;
using NVs.Budget.Domain.Entities.Operations;

namespace NVs.Budget.Application.Contracts.UseCases.Operations;

public record ListOperationsQuery(OperationQuery Query) : IStreamRequest<TrackedOperation>;
