using Microsoft.Extensions.Configuration;
using NVs.Budget.Controllers.Console.Contracts.IO.Options;
using NVs.Budget.Controllers.Console.IO.Output;

namespace NVs.Budget.Controllers.Console.IO.Options;

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
