namespace NVs.Budget.Controllers.Console.Contracts.IO.Output;

public interface IOutputStreamProvider
{
    Task<StreamWriter> GetOutput(string name = "");
    Task<StreamWriter> GetError(string name = "");
}
