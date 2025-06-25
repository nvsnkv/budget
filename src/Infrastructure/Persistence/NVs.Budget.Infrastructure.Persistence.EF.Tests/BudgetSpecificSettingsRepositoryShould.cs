using AutoFixture;
using FluentAssertions;
using FluentResults.Extensions.FluentAssertions;
using NVs.Budget.Application.Contracts.Entities.Budgeting;
using NVs.Budget.Infrastructure.IO.Console.Options;
using NVs.Budget.Infrastructure.Persistence.EF.Repositories;
using NVs.Budget.Infrastructure.Persistence.EF.Tests.Fixtures;

namespace NVs.Budget.Infrastructure.Persistence.EF.Tests;

public class BudgetSpecificSettingsRepositoryShould(DbContextManager manager): IClassFixture<DbContextManager>
{
    private readonly BudgetSpecificSettingsRepository _repo = new(manager.GetDbBudgetContext());
    private readonly Fixture _fixture = manager.TestData.Fixture;
    private readonly TrackedBudget _budget = manager.TestData.Budgets.First();

    [Fact]
    public async Task StoreAndRetrieveSettings()
    {
        var expected = _fixture.Create<CsvReadingOptions>();

        var result = await _repo.UpdateReadingOptionsFor(_budget, expected, CancellationToken.None);
        result.Should().BeSuccess();

        var actual = await _repo.GetReadingOptionsFor(_budget, CancellationToken.None);

        actual.Should().BeEquivalentTo(expected);
    }
}
