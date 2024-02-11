using FluentResults;

namespace NVs.Budget.Application.Services.Accounting.Errors;

internal sealed class AccountBelongsToMultipleOwnersError : IError
{
    public string Message => "Cannot remove account with more than one owner!";
    public Dictionary<string, object> Metadata { get; } = new();
    public List<IError> Reasons { get; } = new();
}
