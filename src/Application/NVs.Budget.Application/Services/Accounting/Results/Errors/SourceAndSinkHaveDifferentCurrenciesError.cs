using FluentResults;
using NVs.Budget.Domain.Entities.Operations;

namespace NVs.Budget.Application.Services.Accounting.Results.Errors;

internal class SourceAndSinkHaveDifferentCurrenciesError(Operation source, Operation sink) : IError
{
    public string Message => "Given source and sink have different currencies!";

    public Dictionary<string, object> Metadata { get; } = new()
    {
        { "Source", source.Id },
        { "Sink", sink.Id }
    };

    public List<IError> Reasons { get; } = new();
}