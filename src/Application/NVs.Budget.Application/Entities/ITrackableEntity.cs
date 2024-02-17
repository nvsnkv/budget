namespace NVs.Budget.Application.Entities;

public interface ITrackableEntity<out T>
{
    T Id { get; }

    string? Version { get; set; }

    bool IsRegistered => Version != null;
}
