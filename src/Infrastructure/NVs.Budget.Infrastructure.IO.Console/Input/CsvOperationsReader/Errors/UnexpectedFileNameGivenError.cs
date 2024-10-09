using FluentResults;

namespace NVs.Budget.Infrastructure.IO.Console.Input.CsvOperationsReader.Errors;

internal class UnexpectedFileNameGivenError(string name) : IError
{
    public string Message { get; } = "Reading configuration for this file is not defined!";
    public Dictionary<string, object> Metadata { get; } = new()
    {
        { nameof(name), name }
    };

    public List<IError> Reasons { get; } = new();
}
