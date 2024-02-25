namespace NVs.Budget.Infrastructure.Storage.Entities;

internal class StoredTag(string value)
{
    public string Value { get; private set; } = value;
}
