using FluentResults;

namespace NVs.Budget.Controllers.Console.Errors;

internal class ExceptionBasedError : IError
{
    public ExceptionBasedError(Exception e)
    {
        Message = e.Message;
        Reasons = new List<IError>();
        Metadata = new Dictionary<string, object>();

        foreach (var key in e.Data.Keys)
        {
            var stringKey = GetKey(key);
            Metadata[stringKey] = e.Data[stringKey] ?? "(null)";
        }

        if (e.InnerException != null)
        {
            Reasons.Add(new ExceptionBasedError(e.InnerException));
        }
    }

    private int keyCounter;

    private string GetKey(object? key) => key as string ?? $"{keyCounter} {key}";

    public string Message { get; }
    public Dictionary<string, object> Metadata { get; }
    public List<IError> Reasons { get; }
}
