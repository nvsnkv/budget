using FluentResults;
using MediatR;
using NVs.Budget.Application.Contracts.Entities.Budgeting;
using NVs.Budget.Application.Contracts.Services;
using NVs.Budget.Application.Contracts.UseCases.Budgets;

namespace NVs.Budget.Application.UseCases.Budgets;

internal class RegisterBudgetCommandHandler(IBudgetManager manager) : IRequestHandler<RegisterBudgetCommand, Result<TrackedBudget>>
{
    public Task<Result<TrackedBudget>> Handle(RegisterBudgetCommand request, CancellationToken cancellationToken) => 
        manager.Register(request.NewBudget, cancellationToken);
}
