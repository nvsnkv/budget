namespace NVs.Budget.Application.Services.Accounting.Duplicates;

public record DuplicatesDetectorSettings(TimeSpan Offset)
{
    public static readonly DuplicatesDetectorSettings Default = new(TimeSpan.FromDays(3));
};
