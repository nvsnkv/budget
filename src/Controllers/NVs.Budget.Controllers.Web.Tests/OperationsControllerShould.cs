using System.Text;
using System.Text.Json;
using FluentAssertions;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Application.Contracts.Results;
using NVs.Budget.Application.Contracts.UseCases.Budgets;
using NVs.Budget.Application.Contracts.UseCases.Operations;
using NVs.Budget.Controllers.Web.Controllers;
using NVs.Budget.Controllers.Web.Models;
using NVs.Budget.Controllers.Web.Utils;
using NVs.Budget.Domain.Entities.Budgets;
using NVs.Budget.Domain.ValueObjects;
using NVs.Budget.Infrastructure.Files.CSV.Contracts;
using NVs.Budget.Utilities.Expressions;

namespace NVs.Budget.Controllers.Web.Tests;

public class OperationsControllerShould
{
    [Fact]
    public async Task ImportOperationsManuallyReadInputAsynchronouslyAndPassOperationsToImportCommand()
    {
        // Arrange
        var budget = CreateBudget();
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<ListOwnedBudgetsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([budget]);

        ImportOperationsCommand? capturedCommand = null;
        List<UnregisteredOperation> importedOperations = [];
        mediator.Setup(m => m.Send(It.IsAny<IRequest<ImportResult>>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<ImportResult>, CancellationToken>((request, _) => capturedCommand = request as ImportOperationsCommand)
            .Returns<IRequest<ImportResult>, CancellationToken>(async (request, _) =>
            {
                var command = (ImportOperationsCommand)request;
                importedOperations = await ToListAsync(command.Operations);
                return CreateImportResult();
            });

        var controller = CreateController(mediator.Object);
        SetJsonBody(controller, """
            [
              {
                "timestamp": "2026-01-10T11:30:00Z",
                "amount": { "value": 1500.45, "currencyCode": "RUB" },
                "description": "Salary",
                "attributes": {
                  "source": "manual",
                  "batch": "payroll"
                }
              },
              {
                "timestamp": "2026-01-11T08:15:00Z",
                "amount": { "value": -25.75, "currencyCode": "USD" },
                "description": "Coffee"
              }
            ]
            """);

        // Act
        var actionResult = await controller.ImportOperationsManually(budget.Id, "budget-v2", "UTC", "Exact", CancellationToken.None);

        // Assert
        var ok = actionResult.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeOfType<ImportResultResponse>();

        capturedCommand.Should().NotBeNull();
        capturedCommand!.Budget.Should().BeSameAs(budget);
        capturedCommand.Options.TransferConfidenceLevel.Should().Be(DetectionAccuracy.Exact);
        budget.Version.Should().Be("budget-v2");

        importedOperations.Should().HaveCount(2);
        importedOperations.Select(o => o.Description).Should().ContainInOrder("Salary", "Coffee");
        importedOperations.Select(o => o.Amount.CurrencyCode.ToString()).Should().ContainInOrder("RUB", "USD");
        importedOperations.First().Attributes.Should().NotBeNull();
        importedOperations.First().Attributes!.Keys.Should().Contain("source");
        importedOperations.First().Timestamp.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public async Task ImportOperationsManuallySplitTransfersByConfidenceLevel()
    {
        // Arrange
        var budget = CreateBudget();
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<ListOwnedBudgetsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([budget]);

        var exact = CreateTrackedTransfer(DetectionAccuracy.Exact);
        var likely = CreateTrackedTransfer(DetectionAccuracy.Likely);
        mediator.Setup(m => m.Send(It.IsAny<IRequest<ImportResult>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ImportResult([], [exact, likely], [], Enumerable.Empty<IReason>()));

        var controller = CreateController(mediator.Object);
        SetJsonBody(controller, "[]");

        // Act
        var actionResult = await controller.ImportOperationsManually(budget.Id, "budget-v2", "UTC", "Exact", CancellationToken.None);

        // Assert
        var ok = actionResult.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ImportResultResponse>().Subject;
        response.RegisteredTransfers.Should().ContainSingle().Which.SourceId.Should().Be(exact.Source.Id);
        response.UnregisteredTransfers.Should().ContainSingle().Which.SourceId.Should().Be(likely.Source.Id);
    }

    [Fact]
    public async Task ImportOperationsManuallyReportAllTransfersAsUnregisteredWithoutConfidenceLevel()
    {
        // Arrange
        var budget = CreateBudget();
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<ListOwnedBudgetsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([budget]);

        var exact = CreateTrackedTransfer(DetectionAccuracy.Exact);
        mediator.Setup(m => m.Send(It.IsAny<IRequest<ImportResult>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ImportResult([], [exact], [], Enumerable.Empty<IReason>()));

        var controller = CreateController(mediator.Object);
        SetJsonBody(controller, "[]");

        // Act
        var actionResult = await controller.ImportOperationsManually(budget.Id, "budget-v2", "UTC", null, CancellationToken.None);

        // Assert
        var ok = actionResult.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ImportResultResponse>().Subject;
        response.RegisteredTransfers.Should().BeEmpty();
        response.UnregisteredTransfers.Should().ContainSingle();
    }

    [Fact]
    public async Task ImportOperationsManuallyReturnBadRequestWhenTransferConfidenceLevelIsInvalid()
    {
        // Arrange
        var budget = CreateBudget();
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<ListOwnedBudgetsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([budget]);

        var controller = CreateController(mediator.Object);
        SetJsonBody(controller, "[]");

        // Act
        var actionResult = await controller.ImportOperationsManually(budget.Id, "budget-v2", "UTC", "NotExistingLevel", CancellationToken.None);

        // Assert
        actionResult.Should().BeOfType<BadRequestObjectResult>();
        mediator.Verify(m => m.Send(It.IsAny<ImportOperationsCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ImportOperationsManuallyReturnBadRequestWhenTimeZoneIsInvalid()
    {
        var budget = CreateBudget();
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<ListOwnedBudgetsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([budget]);

        var controller = CreateController(mediator.Object);
        SetJsonBody(controller, "[]");

        var actionResult = await controller.ImportOperationsManually(budget.Id, "budget-v2", "Not/A/Zone", null, CancellationToken.None);

        actionResult.Should().BeOfType<BadRequestObjectResult>();
        mediator.Verify(m => m.Send(It.IsAny<ImportOperationsCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ImportOperationsManuallyIncludeParsingErrorsInSuccessfulResponse()
    {
        // Arrange
        var budget = CreateBudget();
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<ListOwnedBudgetsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([budget]);

        ImportOperationsCommand? capturedCommand = null;
        List<UnregisteredOperation> importedOperations = [];
        mediator.Setup(m => m.Send(It.IsAny<IRequest<ImportResult>>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<ImportResult>, CancellationToken>((request, _) => capturedCommand = request as ImportOperationsCommand)
            .Returns<IRequest<ImportResult>, CancellationToken>(async (request, _) =>
            {
                var command = (ImportOperationsCommand)request;
                importedOperations = await ToListAsync(command.Operations);
                return CreateImportResult();
            });

        var controller = CreateController(mediator.Object);
        SetJsonBody(controller, """
            [
              {
                "timestamp": "2026-01-10T11:30:00Z",
                "amount": { "value": -42.00, "currencyCode": "RUB" },
                "description": "Valid row"
              },
              {
                "timestamp": "2026-01-11T08:15:00Z",
                "amount": { "value": 100.00, "currencyCode": null },
                "description": "Invalid currency row"
              }
            ]
            """);

        // Act
        var actionResult = await controller.ImportOperationsManually(budget.Id, "budget-v2", "UTC", null, CancellationToken.None);

        // Assert
        var ok = actionResult.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ImportResultResponse>().Subject;
        response.Errors.Should().NotBeEmpty();
        response.Errors.Select(e => e.Message).Should().Contain(message => message.Contains("Invalid money value"));

        capturedCommand.Should().NotBeNull();
        importedOperations.Should().HaveCount(1);
        importedOperations.Single().Description.Should().Be("Valid row");
    }

    private static OperationsController CreateController(IMediator mediator)
    {
        var operationMapper = new OperationMapper(new MoneyMapper());
        var parser = ReadableExpressionsParser.Default;
        var jsonOptions = new JsonOptions();
        jsonOptions.JsonSerializerOptions.Converters.Add(new UtcDateTimeJsonConverter());
        jsonOptions.JsonSerializerOptions.Converters.Add(new NullableUtcDateTimeJsonConverter());

        var controller = new OperationsController(
            mediator,
            operationMapper,
            new TransferMapper(operationMapper, new MoneyMapper()),
            new LogbookMapper(operationMapper),
            parser,
            Mock.Of<ICsvFileReader>(),
            Mock.Of<IReadingSettingsRepository>(),
            new RangeBuilder(),
            Options.Create(jsonOptions));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        return controller;
    }

    private static void SetJsonBody(Controller controller, string body)
    {
        var bytes = Encoding.UTF8.GetBytes(body);
        controller.HttpContext.Request.Body = new MemoryStream(bytes);
        controller.HttpContext.Request.ContentType = "application/json";
    }

    private static ImportResult CreateImportResult(IEnumerable<IReason>? reasons = null)
    {
        return new ImportResult(
            [],
            Array.Empty<TrackedTransfer>(),
            [],
            reasons ?? Enumerable.Empty<IReason>());
    }

    private static TrackedTransfer CreateTrackedTransfer(DetectionAccuracy accuracy)
    {
        var budget = new Domain.Entities.Budgets.Budget(Guid.NewGuid(), "Test budget", [new Owner(Guid.NewGuid(), "Tester")]);
        var source = new TrackedOperation(
            Guid.NewGuid(),
            DateTime.UtcNow,
            new NMoneys.Money(-100m, NMoneys.CurrencyIsoCode.RUB),
            "Source operation",
            string.Empty,
            budget,
            Enumerable.Empty<Tag>(),
            null)
        {
            Version = "v1"
        };
        var sink = new TrackedOperation(
            Guid.NewGuid(),
            DateTime.UtcNow,
            new NMoneys.Money(100m, NMoneys.CurrencyIsoCode.RUB),
            "Sink operation",
            string.Empty,
            budget,
            Enumerable.Empty<Tag>(),
            null)
        {
            Version = "v1"
        };

        return new TrackedTransfer(source, sink, "Test transfer") { Accuracy = accuracy };
    }

    private static async Task<List<T>> ToListAsync<T>(IAsyncEnumerable<T> source)
    {
        var list = new List<T>();
        await foreach (var item in source)
        {
            list.Add(item);
        }

        return list;
    }

    private static TrackedBudget CreateBudget()
    {
        return new TrackedBudget(
            Guid.NewGuid(),
            "Test budget",
            [new Owner(Guid.NewGuid(), "Tester")],
            Array.Empty<TaggingCriterion>(),
            Array.Empty<TransferCriterion>(),
            [LogbookCriteria.Universal])
        {
            Version = "budget-v1"
        };
    }
}
