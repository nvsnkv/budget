using FluentAssertions;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NMoneys;
using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Application.Contracts.Results;
using NVs.Budget.Application.Contracts.UseCases.Budgets;
using NVs.Budget.Application.Contracts.UseCases.Operations;
using NVs.Budget.Controllers.Web.Controllers;
using NVs.Budget.Controllers.Web.Models;
using NVs.Budget.Controllers.Web.Utils;
using NVs.Budget.Domain.Entities.Budgets;
using NVs.Budget.Infrastructure.Files.CSV.Contracts;
using NVs.Budget.Utilities.Expressions;

namespace NVs.Budget.Controllers.Web.Tests;

public class BudgetControllerShould
{
    [Fact]
    public async Task RegisterBudgetWithoutDemoData_ShouldNotRunDemoFlow()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        var demoGenerator = new Mock<IDemoBudgetGenerator>();
        var createdBudget = CreateBudget("Budget A", "v1");

        mediator.Setup(m => m.Send(It.IsAny<RegisterBudgetCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(createdBudget));
        mediator.Setup(m => m.Send(It.IsAny<ListOwnedBudgetsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([createdBudget]);

        var controller = CreateController(mediator.Object, demoGenerator.Object);

        // Act
        var actionResult = await controller.RegisterBudget(new RegisterBudgetRequest("Budget A"), CancellationToken.None);

        // Assert
        actionResult.Should().BeOfType<CreatedAtActionResult>();
        mediator.Verify(m => m.Send(It.IsAny<UpdateBudgetCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        mediator.Verify(m => m.Send(It.IsAny<ImportOperationsCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        demoGenerator.Verify(g => g.Generate(It.IsAny<Guid>(), It.IsAny<DateTime>()), Times.Never);
    }

    [Fact]
    public async Task RegisterBudgetWithDemoData_ShouldUpdateCriteriaAndImportOperations()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        var demoGenerator = new Mock<IDemoBudgetGenerator>();

        var createdBudget = CreateBudget("Budget Demo", "v1");
        var refreshedBudget = CreateBudget("Budget Demo", "v2", createdBudget.Id);
        var demoSeed = CreateDemoSeed();

        mediator.Setup(m => m.Send(It.IsAny<RegisterBudgetCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(createdBudget));
        mediator.Setup(m => m.Send(It.IsAny<UpdateBudgetCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());
        mediator.Setup(m => m.Send(It.IsAny<ImportOperationsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ImportResult([], [], [], Array.Empty<IReason>()));
        mediator.Setup(m => m.Send(It.IsAny<ListOwnedBudgetsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([refreshedBudget]);

        demoGenerator.Setup(g => g.Generate(createdBudget.Id, It.IsAny<DateTime>()))
            .Returns(demoSeed);

        var controller = CreateController(mediator.Object, demoGenerator.Object);

        // Act
        var actionResult = await controller.RegisterBudget(new RegisterBudgetRequest("Budget Demo", true), CancellationToken.None);

        // Assert
        var created = actionResult.Should().BeOfType<CreatedAtActionResult>().Subject;
        var response = created.Value.Should().BeOfType<BudgetResponse>().Subject;
        response.Id.Should().Be(createdBudget.Id);

        mediator.Verify(m => m.Send(It.IsAny<UpdateBudgetCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        mediator.Verify(m => m.Send(It.IsAny<ImportOperationsCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        demoGenerator.Verify(g => g.Generate(createdBudget.Id, It.IsAny<DateTime>()), Times.Once);
    }

    [Fact]
    public async Task GetTaggingCriteria_ShouldReturnSlimResponse()
    {
        var mediator = new Mock<IMediator>();
        var demoGenerator = new Mock<IDemoBudgetGenerator>();
        var budget = CreateBudget("Budget A", "v2");

        mediator.Setup(m => m.Send(It.IsAny<ListOwnedBudgetsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([budget]);

        var controller = CreateController(mediator.Object, demoGenerator.Object);

        var result = await controller.GetTaggingCriteria(budget.Id, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var payload = ok.Value.Should().BeOfType<TaggingCriteriaConfigResponse>().Subject;
        payload.BudgetId.Should().Be(budget.Id);
        payload.Version.Should().Be("v2");
        payload.TaggingCriteria.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateTransferCriteria_ShouldForwardVersionAndUpdateBudget()
    {
        var mediator = new Mock<IMediator>();
        var demoGenerator = new Mock<IDemoBudgetGenerator>();
        var budget = CreateBudget("Budget A", "old-version");

        mediator.Setup(m => m.Send(It.IsAny<ListOwnedBudgetsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([budget]);
        mediator.Setup(m => m.Send(It.IsAny<UpdateBudgetCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        var controller = CreateController(mediator.Object, demoGenerator.Object);
        var request = new UpdateTransferCriteriaRequest
        {
            BudgetId = budget.Id,
            Version = "new-version",
            TransferCriteria =
            [
                new TransferCriterionResponse(
                    DetectionAccuracy.Exact.ToString(),
                    "test transfer",
                    "(source, sink) => source.Amount.Amount == sink.Amount.Amount * -1")
            ]
        };

        var result = await controller.UpdateTransferCriteria(budget.Id, request, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
        mediator.Verify(m => m.Send(
            It.Is<UpdateBudgetCommand>(c =>
                c.Budget.Id == budget.Id &&
                c.Budget.Version == "new-version" &&
                c.Budget.TransferCriteria.Count == 1),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateLogbookCriterion_ShouldReturnNotFoundWhenNameIsMissing()
    {
        var mediator = new Mock<IMediator>();
        var demoGenerator = new Mock<IDemoBudgetGenerator>();
        var budget = CreateBudget("Budget A", "v1");

        mediator.Setup(m => m.Send(It.IsAny<ListOwnedBudgetsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([budget]);

        var controller = CreateController(mediator.Object, demoGenerator.Object);
        var request = new UpdateLogbookCriteriaRequest
        {
            BudgetId = budget.Id,
            Version = "v3",
            LogbookCriteria = new LogbookCriteriaResponse
            {
                Description = "Updated",
                IsUniversal = true
            }
        };

        var result = await controller.UpdateLogbookCriterion(
            budget.Id,
            "missing-name",
            request,
            CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
        mediator.Verify(m => m.Send(It.IsAny<UpdateBudgetCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static BudgetController CreateController(IMediator mediator, IDemoBudgetGenerator demoGenerator)
    {
        var mapper = new BudgetMapper(ReadableExpressionsParser.Default);
        return new BudgetController(
            mediator,
            mapper,
            demoGenerator,
            Mock.Of<IReadingSettingsRepository>(),
            new FileReadingSettingsMapper()
        );
    }

    private static TrackedBudget CreateBudget(string name, string version, Guid? id = null)
    {
        return new TrackedBudget(
            id ?? Guid.NewGuid(),
            name,
            [new Owner(Guid.NewGuid(), "Tester")],
            Array.Empty<TaggingCriterion>(),
            Array.Empty<TransferCriterion>(),
            [LogbookCriteria.Universal])
        {
            Version = version
        };
    }

    private static DemoBudgetSeed CreateDemoSeed()
    {
        var parser = ReadableExpressionsParser.Default;
        var tagging = new[]
        {
            new TaggingCriterion(
                parser.ParseUnaryConversion<TrackedOperation>("o => \"Withdraws\"").Value,
                parser.ParseUnaryPredicate<TrackedOperation>("o => o.Amount.Amount < 0").Value)
        };

        var logbook = new[]
        {
            new LogbookCriteria(
                "Demo",
                null,
                null,
                null,
                null,
                null,
                true)
        };

        var operations = new[]
        {
            new UnregisteredOperation(
                DateTime.UtcNow,
                new Money(-100, Currency.Get("RUB")),
                "Demo operation",
                new Dictionary<string, object> { ["category"] = "food" })
        };

        return new DemoBudgetSeed(tagging, logbook, operations);
    }
}
