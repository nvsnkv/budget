using System.Globalization;
using System.Runtime.CompilerServices;
using CommandLine;
using FluentResults;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Controllers.Console.Criteria;
using NVs.Budget.Controllers.Console.IO;

[assembly:InternalsVisibleTo("NVs.Budget.Controllers.Console.Tests")]

namespace NVs.Budget.Controllers.Console;

public static class ConsoleControllersExtensions
{
    public static IServiceCollection AddConsole(this IServiceCollection services)
    {
        services.AddMediatR(c => c.RegisterServicesFromAssemblyContaining<EntryPoint>());

        services.AddTransient<ResultWriter<Result<TrackedOwner>>, OwnerResultWriter>();

        services.AddTransient<IEntryPoint, EntryPoint>();
        services.AddTransient<Parser>();

        var streams = new OutputStreams(System.Console.Out, System.Console.Error);
        services.AddSingleton(streams);

        return services;
    }

    public static IServiceCollection UseConsole(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<OutputOptions>(configuration.GetSection(nameof(OutputOptions)).Bind);

        var cultureCode = configuration.GetValue<string>("CultureCode");
        var culture = cultureCode is null ? CultureInfo.CurrentCulture : CultureInfo.GetCultureInfo(cultureCode);

        services.AddSingleton<Action<ParserSettings>>(settings =>
        {
            settings.AutoHelp = true;
            settings.AutoVersion = true;
            settings.CaseInsensitiveEnumValues = true;
            settings.IgnoreUnknownArguments = true;
            settings.ParsingCulture = culture;
            settings.EnableDashDash = true;
        });

        var criteriaListReader = new CriteriaListReader(new CriteriaParser());

        var transferCriteria = criteriaListReader.GetTransferCriteria(configuration.GetSection("Transfers"));
        services.AddSingleton(transferCriteria);

        var taggingCriteria = criteriaListReader.GetTaggingCriteria(configuration.GetSection("Tags"));
        services.AddSingleton(taggingCriteria);

        return services;
    }
}
