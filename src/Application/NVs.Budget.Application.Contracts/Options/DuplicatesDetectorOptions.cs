namespace NVs.Budget.Application.Contracts.Options;

public record DuplicatesDetectorOptions(TimeSpan Offset)
{
    public static readonly DuplicatesDetectorOptions Default = new(TimeSpan.FromDays(3));
}
