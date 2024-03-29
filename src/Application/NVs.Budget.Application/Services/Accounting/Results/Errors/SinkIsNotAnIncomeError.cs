﻿using FluentResults;
using NVs.Budget.Domain.Entities.Operations;

namespace NVs.Budget.Application.Services.Accounting.Results.Errors;

internal class SinkIsNotAnIncomeError(Operation sink): IError
{
    public string Message => "Given sink is not an income!";
    public Dictionary<string, object> Metadata { get; } = new() { { "Sink", sink.Id } };
    public List<IError> Reasons { get; } = new();
}
