namespace NVs.Budget.Infrastructure.IO.Console.Options;

public interface IOutputOptionsChanger
{
    void SetOutputStreamName(string output);
    void SetErrorStreamName(string error);
}
