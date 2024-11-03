namespace NVs.Budget.Infrastructure.IO.Console.Output;

public interface IOutputStreamProvider
{
    Task<StreamWriter> GetOutput(string name = "");
    Task<StreamWriter> GetError(string name = "");
    Task ReleaseStreamsAsync();
}
