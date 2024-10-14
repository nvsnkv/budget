using FluentResults;
using MediatR;
using NVs.Budget.Application.Contracts.Entities.Budgeting;

namespace NVs.Budget.Application.Contracts.UseCases.Operations;

public record UpdateOperationsCommand(IAsyncEnumerable<TrackedOperation> Operations) : IRequest<Result>;
