using FluentResults;

namespace NVs.Budget.Infrastructure.Files.CSV.Errors;

internal class RowNotParsedError(int row, List<IError> reasons) : IError
{
    public string Message => "Unable to parse row!";
    public Dictionary<string, object> Metadata { get; } = new() { { nameof(row), row } };
    public List<IError> Reasons { get; } = reasons.ToList();
}

