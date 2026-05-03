using FluentAssertions;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NMoneys;
using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Application.Contracts.Entities.Planning;
using NVs.Budget.Application.Contracts.UseCases.BudgetPlans;
using NVs.Budget.Controllers.Web.Controllers;
using NVs.Budget.Controllers.Web.Models;
using NVs.Budget.Controllers.Web.Utils;
using NVs.Budget.Domain.Aggregates;
using NVs.Budget.Domain.Aggregates.Plans;
using NVs.Budget.Domain.Entities.Plans;
using NVs.Budget.Domain.ValueObjects.Criteria;
using NVs.Budget.Utilities.Expressions;

namespace NVs.Budget.Controllers.Web.Tests;

public class BudgetPlansControllerShould
{
    private static readonly Currency Currency = Currency.Get(CurrencyIsoCode.RUB);

    [Fact]
    public async Task CreatePlanThroughUseCase()
    {
        var budgetId = Guid.NewGuid();
        var mediator = new Mock<IMediator>();
        var controller = CreateController(mediator.Object);
        var created = TrackedPlan(budgetId);

        mediator.Setup(m => m.Send(It.IsAny<RegisterBudgetPlanCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(created));

        var result = await controller.Create(budgetId, Request(), CancellationToken.None);

        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.Value.Should().BeOfType<BudgetPlanResponse>();
        mediator.Verify(m => m.Send(
            It.Is<RegisterBudgetPlanCommand>(c => c.BudgetId == budgetId && c.Plan.Name == "Plan"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ReturnVarianceReport()
    {
        var budgetId = Guid.NewGuid();
        var plan = TrackedPlan(budgetId);
        var mediator = new Mock<IMediator>();
        var controller = CreateController(mediator.Object);
        var logbook = new CriteriaBasedLogbook(plan.Criterion);
        var report = new VarianceReport(plan, logbook, [new PlanRange("January", plan.From, plan.Till)]);

        mediator.Setup(m => m.Send(It.IsAny<BuildBudgetPlanVarianceReportQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(report));

        var result = await controller.Variance(budgetId, plan.Id, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<VarianceReportResponse>().Subject;
        response.Plan.Id.Should().Be(plan.Id);
        response.Variances.Should().HaveCount(1);
    }

    [Fact]
    public async Task ForwardCopyRequest()
    {
        var budgetId = Guid.NewGuid();
        var plan = TrackedPlan(budgetId);
        var mediator = new Mock<IMediator>();
        var controller = CreateController(mediator.Object);

        mediator.Setup(m => m.Send(It.IsAny<CopyBudgetPlanCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(plan));

        var result = await controller.Copy(budgetId, plan.Id, new CopyBudgetPlanRequest("Copied"), CancellationToken.None);

        result.Should().BeOfType<CreatedAtActionResult>();
        mediator.Verify(m => m.Send(
            It.Is<CopyBudgetPlanCommand>(c => c.BudgetId == budgetId && c.PlanId == plan.Id && c.Name == "Copied"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    private static BudgetPlansController CreateController(IMediator mediator)
    {
        return new BudgetPlansController(mediator, new BudgetMapper(ReadableExpressionsParser.Default));
    }

    private static UpsertBudgetPlanRequest Request()
    {
        var from = new DateTime(2026, 1, 1);
        var till = new DateTime(2026, 2, 1);
        return new UpsertBudgetPlanRequest(
            "Plan",
            null,
            from,
            till,
            null,
            new LogbookCriteriaResponse { Description = "All", IsUniversal = true },
            Currency.IsoCode.ToString(),
            null,
            null,
            [new UpsertPlanExpectationRequest(null, new MoneyResponse(-100, Currency.IsoCode.ToString()), from, till, null, null)]);
    }

    private static TrackedBudgetPlan TrackedPlan(Guid budgetId)
    {
        var from = new DateTime(2026, 1, 1);
        var till = new DateTime(2026, 2, 1);
        var criteria = new LogbookCriteria("All", null, null, null, null, null, true);
        return new TrackedBudgetPlan(
            Guid.NewGuid(),
            budgetId,
            "Plan",
            from,
            till,
            null,
            new UniversalCriterion("All"),
            Currency,
            [new PlanExpectation(Guid.NewGuid(), new Money(-100, Currency), from, till)],
            null,
            null,
            criteria)
        {
            Version = "v1"
        };
    }
}
