namespace NVs.Budget.Application.Contracts.Entities;

public interface ITrackableEntity<out T>
{
    T Id { get; }

    string? Version { get; set; }

    bool IsRegistered => Version != null;
}
