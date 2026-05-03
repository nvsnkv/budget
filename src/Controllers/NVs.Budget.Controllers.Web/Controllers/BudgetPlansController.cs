using Asp.Versioning;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NMoneys;
using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Application.Contracts.Entities.Planning;
using NVs.Budget.Application.Contracts.UseCases.BudgetPlans;
using NVs.Budget.Controllers.Web.Models;
using NVs.Budget.Controllers.Web.Utils;
using NVs.Budget.Domain.Aggregates.Plans;
using NVs.Budget.Domain.Entities.Plans;

namespace NVs.Budget.Controllers.Web.Controllers;

[Authorize]
[ApiVersion("0.1")]
[Route("api/v{version:apiVersion}/budget/{budgetId:guid}/plans")]
[Produces("application/json")]
public class BudgetPlansController(IMediator mediator, BudgetMapper budgetMapper) : Controller
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<BudgetPlanResponse>), 200)]
    public async Task<IActionResult> List([FromRoute] Guid budgetId, CancellationToken ct)
    {
        var plans = await mediator.Send(new ListBudgetPlansQuery(budgetId), ct);
        return Ok(plans.Select(ToResponse).ToList());
    }

    [HttpGet("{planId:guid}")]
    [ProducesResponseType(typeof(BudgetPlanResponse), 200)]
    [ProducesResponseType(typeof(IEnumerable<IError>), 404)]
    public async Task<IActionResult> Get([FromRoute] Guid budgetId, [FromRoute] Guid planId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetBudgetPlanQuery(budgetId, planId), ct);
        return ToActionResult(result, ToResponse);
    }

    [HttpPost]
    [ProducesResponseType(typeof(BudgetPlanResponse), 201)]
    [ProducesResponseType(typeof(IEnumerable<IError>), 400)]
    public async Task<IActionResult> Create([FromRoute] Guid budgetId, [FromBody] UpsertBudgetPlanRequest request, CancellationToken ct)
    {
        var planResult = ToUnregisteredPlan(request);
        if (planResult.IsFailed)
        {
            return BadRequest(planResult.Errors);
        }

        var result = await mediator.Send(new RegisterBudgetPlanCommand(budgetId, planResult.Value), ct);
        if (result.IsFailed)
        {
            return BadRequest(result.Errors);
        }

        return CreatedAtAction(nameof(Get), new { budgetId, planId = result.Value.Id }, ToResponse(result.Value));
    }

    [HttpPut("{planId:guid}")]
    [ProducesResponseType(typeof(BudgetPlanResponse), 200)]
    [ProducesResponseType(typeof(IEnumerable<IError>), 400)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid budgetId,
        [FromRoute] Guid planId,
        [FromBody] UpsertBudgetPlanRequest request,
        CancellationToken ct)
    {
        var planResult = ToTrackedPlan(budgetId, planId, request);
        if (planResult.IsFailed)
        {
            return BadRequest(planResult.Errors);
        }

        var result = await mediator.Send(new UpdateBudgetPlanCommand(budgetId, planResult.Value), ct);
        return ToActionResult(result, ToResponse);
    }

    [HttpDelete("{planId:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(IEnumerable<IError>), 400)]
    public async Task<IActionResult> Delete([FromRoute] Guid budgetId, [FromRoute] Guid planId, CancellationToken ct)
    {
        var result = await mediator.Send(new RemoveBudgetPlanCommand(budgetId, planId), ct);
        return result.IsSuccess ? NoContent() : BadRequest(result.Errors);
    }

    [HttpPost("{planId:guid}/copy")]
    [ProducesResponseType(typeof(BudgetPlanResponse), 201)]
    [ProducesResponseType(typeof(IEnumerable<IError>), 400)]
    public async Task<IActionResult> Copy([FromRoute] Guid budgetId, [FromRoute] Guid planId, [FromBody] CopyBudgetPlanRequest? request, CancellationToken ct)
    {
        var result = await mediator.Send(new CopyBudgetPlanCommand(budgetId, planId, request?.Name), ct);
        if (result.IsFailed)
        {
            return BadRequest(result.Errors);
        }

        return CreatedAtAction(nameof(Get), new { budgetId, planId = result.Value.Id }, ToResponse(result.Value));
    }

    [HttpGet("{planId:guid}/variance")]
    [ProducesResponseType(typeof(VarianceReportResponse), 200)]
    [ProducesResponseType(typeof(IEnumerable<IError>), 400)]
    public async Task<IActionResult> Variance([FromRoute] Guid budgetId, [FromRoute] Guid planId, CancellationToken ct)
    {
        var result = await mediator.Send(new BuildBudgetPlanVarianceReportQuery(budgetId, planId), ct);
        return ToActionResult(result, ToResponse);
    }

    private Result<UnregisteredBudgetPlan> ToUnregisteredPlan(UpsertBudgetPlanRequest request)
    {
        var criteriaResult = budgetMapper.FromRequest(request.LogbookCriteria);
        if (criteriaResult.IsFailed)
        {
            return Result.Fail<UnregisteredBudgetPlan>(criteriaResult.Errors);
        }

        var currencyResult = ParseCurrency(request.CurrencyCode);
        if (currencyResult.IsFailed)
        {
            return Result.Fail<UnregisteredBudgetPlan>(currencyResult.Errors);
        }

        var expectations = ToExpectations(request.Expectations, currencyResult.Value);
        if (expectations.IsFailed)
        {
            return Result.Fail<UnregisteredBudgetPlan>(expectations.Errors);
        }

        return new UnregisteredBudgetPlan(
            request.Name,
            InboundUtcDateTime.Normalize(request.From),
            InboundUtcDateTime.Normalize(request.Till),
            request.CronExpression,
            criteriaResult.Value.GetCriterion(),
            currencyResult.Value,
            expectations.Value,
            request.ExpectedAmount is null ? null : new Money(request.ExpectedAmount.Value, currencyResult.Value),
            request.Note,
            criteriaResult.Value);
    }

    private Result<TrackedBudgetPlan> ToTrackedPlan(Guid budgetId, Guid planId, UpsertBudgetPlanRequest request)
    {
        var unregistered = ToUnregisteredPlan(request);
        if (unregistered.IsFailed)
        {
            return Result.Fail<TrackedBudgetPlan>(unregistered.Errors);
        }

        return new TrackedBudgetPlan(
            planId,
            budgetId,
            unregistered.Value.Name,
            unregistered.Value.From,
            unregistered.Value.Till,
            unregistered.Value.CronExpression,
            unregistered.Value.Criterion,
            unregistered.Value.Currency,
            unregistered.Value.Expectations,
            unregistered.Value.ExpectedAmount,
            unregistered.Value.Note,
            unregistered.Value.LogbookCriteria)
        {
            Version = request.Version
        };
    }

    private static Result<IReadOnlyCollection<PlanExpectation>> ToExpectations(
        IReadOnlyCollection<UpsertPlanExpectationRequest>? requests,
        Currency currency)
    {
        try
        {
            return Result.Ok<IReadOnlyCollection<PlanExpectation>>((requests ?? [])
                .Select(e => new PlanExpectation(
                    e.Id ?? Guid.NewGuid(),
                    new Money(e.ExpectedAmount.Value, currency),
                    InboundUtcDateTime.Normalize(e.From),
                    InboundUtcDateTime.Normalize(e.Till),
                    e.SubcriterionName,
                    e.Note))
                .ToList());
        }
        catch (Exception e)
        {
            return Result.Fail<IReadOnlyCollection<PlanExpectation>>(e.Message);
        }
    }

    private static Result<Currency> ParseCurrency(string currencyCode)
    {
        try
        {
            return Currency.Get(currencyCode);
        }
        catch (Exception e)
        {
            return Result.Fail<Currency>($"Invalid currency code: {currencyCode}. {e.Message}");
        }
    }

    private BudgetPlanResponse ToResponse(TrackedBudgetPlan plan)
    {
        var criteria = plan.LogbookCriteria ?? LogbookCriteria.Universal;
        return new BudgetPlanResponse(
            plan.Id,
            plan.BudgetId,
            plan.Name,
            plan.Version ?? string.Empty,
            plan.From,
            plan.Till,
            plan.CronExpression,
            budgetMapper.ToResponse(criteria),
            plan.Currency.IsoCode.ToString(),
            ToMoneyResponse(plan.ExpectedAmount),
            plan.Note,
            plan.Expectations.Select(ToResponse).ToList());
    }

    private static PlanExpectationResponse ToResponse(PlanExpectation expectation)
    {
        return new PlanExpectationResponse(
            expectation.Id,
            ToMoneyResponse(expectation.ExpectedAmount),
            expectation.From,
            expectation.Till,
            expectation.SubcriterionName,
            expectation.Note);
    }

    private VarianceReportResponse ToResponse(VarianceReport report)
    {
        return new VarianceReportResponse(ToResponse((TrackedBudgetPlan)report.Plan), report.Variances.Select(ToResponse).ToList());
    }

    private static VarianceResponse ToResponse(Variance variance)
    {
        return new VarianceResponse(
            variance.Description,
            new NamedRangeResponse(variance.Range.Name, variance.Range.From, variance.Range.Till),
            ToMoneyResponse(variance.Expected),
            ToMoneyResponse(variance.Actual),
            ToMoneyResponse(variance.Difference),
            variance.HasPlan,
            variance.HasActual,
            variance.IsPlannedOnly,
            variance.ActualComparison,
            variance.Children.Select(ToResponse).ToList());
    }

    private static MoneyResponse ToMoneyResponse(Money money)
    {
        return new MoneyResponse(money.Amount, money.GetCurrency().IsoCode.ToString());
    }

    private IActionResult ToActionResult<T, TResponse>(Result<T> result, Func<T, TResponse> map)
    {
        return result.IsSuccess ? Ok(map(result.Value)) : BadRequest(result.Errors);
    }
}
