namespace NVs.Budget.Infrastructure.Storage.Repositories;

internal class VersionGenerator
{
    private readonly Random _random = new();

    public string Next() => _random.NextInt64().ToString("x8");
}
