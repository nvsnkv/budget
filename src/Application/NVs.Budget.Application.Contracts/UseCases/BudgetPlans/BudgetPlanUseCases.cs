using FluentResults;
using MediatR;
using NVs.Budget.Application.Contracts.Entities.Planning;
using NVs.Budget.Domain.Aggregates.Plans;

namespace NVs.Budget.Application.Contracts.UseCases.BudgetPlans;

public record ListBudgetPlansQuery(Guid BudgetId) : IRequest<IReadOnlyCollection<TrackedBudgetPlan>>;

public record GetBudgetPlanQuery(Guid BudgetId, Guid PlanId) : IRequest<Result<TrackedBudgetPlan>>;

public record RegisterBudgetPlanCommand(Guid BudgetId, UnregisteredBudgetPlan Plan) : IRequest<Result<TrackedBudgetPlan>>;

public record UpdateBudgetPlanCommand(Guid BudgetId, TrackedBudgetPlan Plan) : IRequest<Result<TrackedBudgetPlan>>;

public record RemoveBudgetPlanCommand(Guid BudgetId, Guid PlanId) : IRequest<Result>;

public record CopyBudgetPlanCommand(Guid BudgetId, Guid PlanId, string? Name = null) : IRequest<Result<TrackedBudgetPlan>>;

public record BuildBudgetPlanVarianceReportQuery(Guid BudgetId, Guid PlanId) : IRequest<Result<VarianceReport>>;
