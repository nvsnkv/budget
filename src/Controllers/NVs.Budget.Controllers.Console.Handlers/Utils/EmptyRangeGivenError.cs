using FluentResults;

namespace NVs.Budget.Controllers.Console.Handlers.Utils;

internal class EmptyRangeGivenError : IError
{
    public string Message => "Incorrect schedule given: no scheduled occurence belongs to given date range! At least 2 occurences required to ";
    public Dictionary<string, object> Metadata { get; } = new();
    public List<IError> Reasons { get; } = new();
}