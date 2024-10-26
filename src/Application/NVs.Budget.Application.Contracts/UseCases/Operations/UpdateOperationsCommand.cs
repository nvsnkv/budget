using FluentResults;
using MediatR;
using NVs.Budget.Application.Contracts.Entities.Budgeting;
using NVs.Budget.Application.Contracts.Options;
using NVs.Budget.Application.Contracts.Results;

namespace NVs.Budget.Application.Contracts.UseCases.Operations;

public record UpdateOperationsCommand(
    IAsyncEnumerable<TrackedOperation> Operations,
    TrackedBudget Budget,
    UpdateOptions Options
) : IRequest<UpdateResult>;
