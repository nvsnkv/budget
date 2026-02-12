using System.Text.RegularExpressions;
using FluentResults;
using NVs.Budget.Infrastructure.Files.CSV.Contracts;

namespace NVs.Budget.Controllers.Web.IntegrationTests.Infrastructure;

internal sealed class StubReadingSettingsRepository : IReadingSettingsRepository
{
    public Task<IReadOnlyDictionary<Regex, FileReadingSetting>> GetReadingSettingsFor(TrackedBudget budget, CancellationToken ct)
    {
        IReadOnlyDictionary<Regex, FileReadingSetting> result = new Dictionary<Regex, FileReadingSetting>();
        return Task.FromResult(result);
    }

    public Task<Result> UpdateReadingSettingsFor(TrackedBudget budget, IReadOnlyDictionary<Regex, FileReadingSetting> settings, CancellationToken ct)
    {
        return Task.FromResult(Result.Ok());
    }
}

internal sealed class StubCsvFileReader : ICsvFileReader
{
    public IAsyncEnumerable<Result<UnregisteredOperation>> ReadUntrackedOperations(StreamReader reader, FileReadingSetting config, CancellationToken ct)
    {
        return AsyncEnumerable.Empty<Result<UnregisteredOperation>>();
    }
}
