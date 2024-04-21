using CommandLine;
using MediatR;

namespace NVs.Budget.Controllers.Console.Contracts.Commands;

public class AbstractVerb : IRequest<ExitCode>
{
    [Option('o', "output", Required = false, HelpText = "Output path. If no value is set, app will use value from configuration")]
    public string? OutputPath { get; set; }

    [Option('e', "errors", Required = false, HelpText = "Errors path. If no value is set, app will use value from configuration")]
    public string? ErrorsPath { get; set; }

}
