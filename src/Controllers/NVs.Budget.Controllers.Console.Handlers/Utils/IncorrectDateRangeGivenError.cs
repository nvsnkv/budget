using FluentResults;

namespace NVs.Budget.Controllers.Console.Handlers.Utils;

internal class IncorrectDateRangeGivenError : IError
{
    public string Message => "Incorrect date range given!";
    public Dictionary<string, object> Metadata { get; } = new();
    public List<IError> Reasons { get; } = new();
}