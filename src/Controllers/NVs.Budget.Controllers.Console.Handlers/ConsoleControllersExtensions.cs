using System.Globalization;
using System.Runtime.CompilerServices;
using CommandLine;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NVs.Budget.Controllers.Console.Contracts.Commands;
using NVs.Budget.Controllers.Console.Handlers.Behaviors;
using NVs.Budget.Controllers.Console.Handlers.Commands;
using NVs.Budget.Controllers.Console.Handlers.Criteria;
using NVs.Budget.Controllers.Console.Handlers.Criteria.Logbook;
using NVs.Budget.Controllers.Console.Handlers.Utils;
using NVs.Budget.Utilities.MediatR;

[assembly:InternalsVisibleTo("NVs.Budget.Controllers.Console.Handlers.Tests")]

namespace NVs.Budget.Controllers.Console.Handlers;

public static class ConsoleControllersExtensions
{
    public static IServiceCollection AddConsole(this IServiceCollection services)
    {
        services.AddMediatR(c =>
        {
            c.RegisterServicesFromAssemblyContaining<EntryPoint>();
            c.AddOpenRequestPostProcessor(typeof(ResultWritingPostProcessor<,>));
        });
        services.AddTransient<IRequestHandler<SuperVerb, ExitCode>, SuperVerbHandler<SuperVerb>>();
        services.EmpowerMediatRHandlersFor(typeof(IRequestHandler<,>));

        services.AddTransient<IEntryPoint, EntryPoint>();
        services.AddTransient<Parser>();
        services.AddTransient<CriteriaParser>();
        services.AddTransient<CronBasedNamedRangeSeriesBuilder>();

        return services;
    }

    public static IServiceCollection UseConsole(this IServiceCollection services, IConfiguration configuration)
    {
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

        var criteriaParser = new CriteriaParser();
        var substitutionsParser = new SubstitutionsParser(criteriaParser);
        var criteriaListReader = new CriteriaListReader(criteriaParser, substitutionsParser, configuration);

        var transferCriteria = criteriaListReader.GetTransferCriteria();
        services.AddSingleton(transferCriteria);

        var taggingCriteria = criteriaListReader.GetTaggingCriteria();
        services.AddSingleton(taggingCriteria);

        var logbookCriteriaReader = new YamlLogbookRulesetReader(criteriaParser, substitutionsParser);
        services.AddSingleton(logbookCriteriaReader);

        return services;
    }
}
