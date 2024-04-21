namespace NVs.Budget.Controllers.Console.Contracts.IO.Options;

public interface IOutputOptionsChanger
{
    void SetOutputStreamName(string output);
    void SetErrorStreamName(string error);
}
