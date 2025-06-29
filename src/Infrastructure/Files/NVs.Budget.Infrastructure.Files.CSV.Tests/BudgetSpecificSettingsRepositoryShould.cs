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
public class BudgetSpecificSettingsRepositoryShould : IClassFixture<DbContextManager>
{
    private readonly BudgetSpecificSettingsRepository _repository;
    private readonly Fixture _fixture;

    public BudgetSpecificSettingsRepositoryShould(DbContextManager manager)
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

    [Fact]
    public async Task Should_Not_Return_Old_Configs_After_Update()
    {
        var budget = _fixture.Create<TrackedBudget>();
        var oldSettings = _fixture.Create<Dictionary<Regex, FileReadingSetting>>();
        var newSettings = _fixture.Create<Dictionary<Regex, FileReadingSetting>>();

        await _repository.UpdateReadingSettingsFor(budget, oldSettings, CancellationToken.None);
        var firstRetrieval = await _repository.GetReadingSettingsFor(budget, CancellationToken.None);
        firstRetrieval.ToDictionary(x => x.Key.ToString(), x => x.Value)
            .Should().BeEquivalentTo(oldSettings.ToDictionary(x => x.Key.ToString(), x => x.Value));


        await _repository.UpdateReadingSettingsFor(budget, newSettings, CancellationToken.None);
        var secondRetrieval = await _repository.GetReadingSettingsFor(budget, CancellationToken.None);
        secondRetrieval.ToDictionary(x => x.Key.ToString(), x => x.Value)
            .Should().BeEquivalentTo(newSettings.ToDictionary(x => x.Key.ToString(), x => x.Value));

        secondRetrieval.ToDictionary(x => x.Key.ToString(), x => x.Value)
            .Should().NotBeEquivalentTo(oldSettings.ToDictionary(x => x.Key.ToString(), x => x.Value));
    }

    [Fact]
    public async Task Should_Retrieve_Settings_Only_For_Assigned_Budget()
    {
        var budget1 = _fixture.Create<TrackedBudget>();
        var settings1 = _fixture.Create<Dictionary<Regex, FileReadingSetting>>();

        var budget2 = _fixture.Create<TrackedBudget>();
        var settings2 = _fixture.Create<Dictionary<Regex, FileReadingSetting>>();

        await _repository.UpdateReadingSettingsFor(budget1, settings1, CancellationToken.None);
        await _repository.UpdateReadingSettingsFor(budget2, settings2, CancellationToken.None);

        var retrievedSettings1 = await _repository.GetReadingSettingsFor(budget1, CancellationToken.None);
        retrievedSettings1.ToDictionary(x => x.Key.ToString(), x => x.Value)
            .Should().BeEquivalentTo(settings1.ToDictionary(x => x.Key.ToString(), x => x.Value));

        var retrievedSettings2 = await _repository.GetReadingSettingsFor(budget2, CancellationToken.None);
        retrievedSettings2.ToDictionary(x => x.Key.ToString(), x => x.Value)
            .Should().BeEquivalentTo(settings2.ToDictionary(x => x.Key.ToString(), x => x.Value));
    }
}
