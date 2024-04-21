using CommandLine;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Controllers.Console.Contracts.Commands;
using NVs.Budget.Controllers.Console.Contracts.IO.Output;
using NVs.Budget.Controllers.Console.IO.Output;
using NVs.Budget.Infrastructure.Persistence.EF.Context;

namespace NVs.Budget.Hosts.Console.Commands;

[Verb("settings", HelpText = "Prints out effective settings")]
internal class ListEffectiveSettingsVerb : IRequest<ExitCode>;

internal class ListEffectiveSettingsVerbHandler(
    IOutputStreamProvider streams,
    IOptionsSnapshot<OutputOptions> outputOptions,
    IConfigurationRoot configuration,
    IDbConnectionInfo dbConnectionInfo,
    IReadOnlyList<TransferCriterion> transferCriteria,
    IReadOnlyCollection<TaggingCriterion> taggingCriteria
) : IRequestHandler<ListEffectiveSettingsVerb, ExitCode>
{
    public async Task<ExitCode> Handle(ListEffectiveSettingsVerb request, CancellationToken cancellationToken)
    {
        var writer = await streams.GetOutput(outputOptions.Value.OutputStreamName);

        await writer.WriteLineAsync("1. Output options:");
        await writer.WriteLineAsync($"{nameof(OutputOptions.OutputStreamName)}: {outputOptions.Value.OutputStreamName}");
        await writer.WriteLineAsync($"{nameof(OutputOptions.ErrorStreamName)}: {outputOptions.Value.ErrorStreamName}");
        await writer.WriteLineAsync($"{nameof(OutputOptions.ShowSuccesses)}: {outputOptions.Value.ShowSuccesses}");
        await writer.WriteLineAsync();

        await writer.WriteLineAsync("2. Configuration sources");
        await writer.WriteLineAsync($"Working directory: {Environment.CurrentDirectory}");
        await writer.WriteLineAsync($"BUDGET_CONFIGURATION_PATH: {Environment.GetEnvironmentVariable("BUDGET_CONFIGURATION_PATH") ?? "conf.d"}");
        await writer.WriteLineAsync("Providers:");
        foreach (var provider in configuration.Providers)
        {
           await writer.WriteLineAsync(provider.ToString());
        }

        await writer.WriteLineAsync();

        await writer.WriteLineAsync("3. EF Core: Database");
        await writer.WriteLineAsync($"{nameof(dbConnectionInfo.DataSource)}: {dbConnectionInfo.DataSource}");
        await writer.WriteLineAsync($"{nameof(dbConnectionInfo.Database)}: {dbConnectionInfo.Database}");
        await writer.WriteLineAsync();

        await writer.WriteLineAsync("4. Transfer criteria");
        foreach (var criterion in transferCriteria)
        {
            await writer.WriteLineAsync($"{criterion.Comment} - {criterion.Accuracy}");
        }

        await writer.WriteLineAsync();

        await writer.WriteLineAsync("5. Tagging criteria");
        foreach (var criterion in taggingCriteria)
        {
            await writer.WriteLineAsync(criterion.Tag.ToString());
        }

        await writer.WriteLineAsync();

        await writer.WriteLineAsync("6. Parsing rules");
        await writer.WriteLineAsync($"CultureCode: {configuration.GetValue<string>("CultureCode") ?? "not specified"}");
        await writer.WriteLineAsync("Known input types:");
        foreach (var section in configuration.GetSection("CsvReadingOptions").GetChildren())
        {
            await writer.WriteLineAsync(section.Key);
        }

        await writer.FlushAsync(cancellationToken);

        return ExitCode.Success;
    }
}
