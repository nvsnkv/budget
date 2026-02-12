using NVs.Budget.Controllers.Web.IntegrationTests.Infrastructure;

namespace NVs.Budget.Controllers.Web.IntegrationTests;

public class BudgetAndLogbookEndpointsShould
{
    [Fact]
    public async Task ReturnNamedLogbooksForBudgetEndpoint()
    {
        var budget = CreateBudgetWithTwoLogbooks();
        await using var factory = new TestApiFactory([budget]);

        var response = await factory.Client.GetAsync($"/api/v0.1/budget/{budget.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<BudgetResponse>();
        payload.Should().NotBeNull();
        payload!.LogbookCriteria.Should().HaveCount(2);
        payload.LogbookCriteria.Select(l => l.Name).Should().Contain(["Default", "Extended"]);
    }

    [Fact]
    public async Task PersistIndependentLogbookEditsViaUpdateEndpoint()
    {
        var budget = CreateBudgetWithTwoLogbooks();
        await using var factory = new TestApiFactory([budget]);

        var request = new UpdateBudgetRequest
        {
            Name = budget.Name,
            Version = budget.Version!,
            LogbookCriteria =
            [
                new LogbookCriteriaResponse
                {
                    CriteriaId = budget.LogbookCriteria.First().CriteriaId,
                    Name = "Default",
                    Description = "Edited default",
                    IsUniversal = true
                },
                new LogbookCriteriaResponse
                {
                    CriteriaId = budget.LogbookCriteria.Last().CriteriaId,
                    Name = "Extended",
                    Description = budget.LogbookCriteria.Last().Description,
                    IsUniversal = true
                }
            ]
        };

        var updateResponse = await factory.Client.PutAsJsonAsync($"/api/v0.1/budget/{budget.Id}", request);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var readResponse = await factory.Client.GetAsync($"/api/v0.1/budget/{budget.Id}");
        var payload = await readResponse.Content.ReadFromJsonAsync<BudgetResponse>();
        payload.Should().NotBeNull();
        payload!.LogbookCriteria.Single(l => l.Name == "Default").Description.Should().Be("Edited default");
        payload.LogbookCriteria.Single(l => l.Name == "Extended").Description.Should().Be("Extended criteria");
    }

    [Fact]
    public async Task SelectCriteriaByLogbookIdAndRejectUnknownId()
    {
        var budget = CreateBudgetWithTwoLogbooks();
        await using var factory = new TestApiFactory([budget]);
        var selected = budget.LogbookCriteria.Last();

        var missing = await factory.Client.GetAsync($"/api/v0.1/budget/{budget.Id}/operations/logbook");
        missing.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var valid = await factory.Client.GetAsync($"/api/v0.1/budget/{budget.Id}/operations/logbook?logbookId={selected.CriteriaId}");
        valid.StatusCode.Should().Be(HttpStatusCode.OK);

        var invalid = await factory.Client.GetAsync($"/api/v0.1/budget/{budget.Id}/operations/logbook?logbookId={Guid.NewGuid()}");
        invalid.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private static TrackedBudget CreateBudgetWithTwoLogbooks()
    {
        var owner = new NVs.Budget.Domain.Entities.Budgets.Owner(Guid.NewGuid(), "Integration Owner");
        var defaultLogbook = new LogbookCriteria(Guid.NewGuid(), "Default", "Default criteria", null, null, null, null, null, true);
        var extendedLogbook = new LogbookCriteria(Guid.NewGuid(), "Extended", "Extended criteria", null, null, null, null, null, true);

        return new TrackedBudget(
            Guid.NewGuid(),
            "Integration Budget",
            [owner],
            [],
            [],
            [defaultLogbook, extendedLogbook])
        {
            Version = "integration-v1"
        };
    }
}
