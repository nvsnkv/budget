using System.Text.RegularExpressions;
using AutoFixture;
using FluentAssertions;
using FluentResults.Extensions.FluentAssertions;
using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Application.Contracts.Entities.Budgeting;
using NVs.Budget.Infrastructure.Files.CSV.Contracts;
using NVs.Budget.Infrastructure.Files.CSV.Persistence.Settings;
using NVs.Budget.Utilities.Testing;

namespace NVs.Budget.Infrastructure.Files.CSV.Tests;

[Collection(nameof(DatabaseCollectionFixture))]
public class UnitTest1 : IClassFixture<DbContextManager>
{
    private readonly BudgetSpecificSettingsRepository _repository;
    private readonly Fixture _fixture;

    public UnitTest1(DbContextManager manager)
    {
        _repository = new BudgetSpecificSettingsRepository(manager.GetSettingsContext());
        _fixture = new Fixture() { Customizations = { new ReadableExpressionsBuilder() } };
        _fixture.Inject(LogbookCriteria.Universal);
    }

    [Fact]
    public async Task Should_SaveAndRetrieve_FileReadingSettings_Successfully()
    {
        var budget = _fixture.Create<TrackedBudget>();
        var expectedSettings = _fixture.Create<Dictionary<Regex, FileReadingSetting>>();

        var saveResult = await _repository.UpdateReadingSettingsFor(budget, expectedSettings, CancellationToken.None);
        saveResult.Should().BeSuccess();

        var retrievedSettings = await _repository.GetReadingSettingsFor(budget, CancellationToken.None);
        retrievedSettings.Should().NotBeNull();
        retrievedSettings.ToDictionary(x => x.Key.ToString(), x => x.Value)
            .Should().BeEquivalentTo(expectedSettings.ToDictionary(x => x.Key.ToString(), x => x.Value));
    }
}
