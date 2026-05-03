using FluentResults;
using MediatR;
using NVs.Budget.Application.Contracts.Entities.Planning;
using NVs.Budget.Application.Contracts.Services;
using NVs.Budget.Application.Contracts.UseCases.BudgetPlans;
using NVs.Budget.Domain.Aggregates.Plans;

namespace NVs.Budget.Application.UseCases.BudgetPlans;

internal class ListBudgetPlansQueryHandler(IBudgetPlanManager manager)
    : IRequestHandler<ListBudgetPlansQuery, IReadOnlyCollection<TrackedBudgetPlan>>
{
    public Task<IReadOnlyCollection<TrackedBudgetPlan>> Handle(ListBudgetPlansQuery request, CancellationToken cancellationToken) =>
        manager.GetPlans(request.BudgetId, cancellationToken);
}

internal class GetBudgetPlanQueryHandler(IBudgetPlanManager manager)
    : IRequestHandler<GetBudgetPlanQuery, Result<TrackedBudgetPlan>>
{
    public Task<Result<TrackedBudgetPlan>> Handle(GetBudgetPlanQuery request, CancellationToken cancellationToken) =>
        manager.GetPlan(request.BudgetId, request.PlanId, cancellationToken);
}

internal class RegisterBudgetPlanCommandHandler(IBudgetPlanManager manager)
    : IRequestHandler<RegisterBudgetPlanCommand, Result<TrackedBudgetPlan>>
{
    public Task<Result<TrackedBudgetPlan>> Handle(RegisterBudgetPlanCommand request, CancellationToken cancellationToken) =>
        manager.Register(request.BudgetId, request.Plan, cancellationToken);
}

internal class UpdateBudgetPlanCommandHandler(IBudgetPlanManager manager)
    : IRequestHandler<UpdateBudgetPlanCommand, Result<TrackedBudgetPlan>>
{
    public Task<Result<TrackedBudgetPlan>> Handle(UpdateBudgetPlanCommand request, CancellationToken cancellationToken) =>
        manager.Update(request.BudgetId, request.Plan, cancellationToken);
}

internal class RemoveBudgetPlanCommandHandler(IBudgetPlanManager manager)
    : IRequestHandler<RemoveBudgetPlanCommand, Result>
{
    public Task<Result> Handle(RemoveBudgetPlanCommand request, CancellationToken cancellationToken) =>
        manager.Remove(request.BudgetId, request.PlanId, cancellationToken);
}

internal class CopyBudgetPlanCommandHandler(IBudgetPlanManager manager)
    : IRequestHandler<CopyBudgetPlanCommand, Result<TrackedBudgetPlan>>
{
    public Task<Result<TrackedBudgetPlan>> Handle(CopyBudgetPlanCommand request, CancellationToken cancellationToken) =>
        manager.Copy(request.BudgetId, request.PlanId, request.Name, cancellationToken);
}

internal class BuildBudgetPlanVarianceReportQueryHandler(IBudgetPlanManager manager)
    : IRequestHandler<BuildBudgetPlanVarianceReportQuery, Result<VarianceReport>>
{
    public Task<Result<VarianceReport>> Handle(BuildBudgetPlanVarianceReportQuery request, CancellationToken cancellationToken) =>
        manager.BuildVarianceReport(request.BudgetId, request.PlanId, request.TimeZoneId, cancellationToken);
}
