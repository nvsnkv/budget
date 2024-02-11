namespace NVs.Budget.Application.Services.Storage;

public interface ITrackableEntity<out T>
{
    T Id { get; }

    string? Version { get; set; }

    bool IsRegistered => Version != null;
}
