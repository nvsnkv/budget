using Microsoft.Extensions.Configuration;

namespace NVs.Budget.Infrastructure.IO.Console.Options;

internal class OutputOptionsChanger(IConfiguration configuration) : IOutputOptionsChanger
{
    public void SetOutputStreamName(string output)
    {
        configuration.GetSection(nameof(OutputOptions))[nameof(OutputOptions.OutputStreamName)] = output;
    }

    public void SetErrorStreamName(string error)
    {
        configuration.GetSection(nameof(OutputOptions))[nameof(OutputOptions.ErrorStreamName)] = error;
    }
}
