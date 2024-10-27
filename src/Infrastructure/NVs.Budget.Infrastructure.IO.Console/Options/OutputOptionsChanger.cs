using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace NVs.Budget.Infrastructure.IO.Console.Options;

internal class OutputOptionsChanger(IConfiguration configuration, IOptionsMonitor<OutputOptions> monitor) : IOutputOptionsChanger
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
