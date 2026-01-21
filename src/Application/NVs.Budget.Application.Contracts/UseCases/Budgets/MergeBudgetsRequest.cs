using FluentResults;
using MediatR;

namespace NVs.Budget.Application.Contracts.UseCases.Budgets;

public record MergeBudgetsRequest(IReadOnlyList<Guid> BudgetIds, bool PurgeEmptyBudgets) : IRequest<Result>;
