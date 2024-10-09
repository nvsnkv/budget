using System.Globalization;
using System.Runtime.CompilerServices;
using CsvHelper.Configuration;
using FluentResults;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Domain.Entities.Operations;
using NVs.Budget.Infrastructure.IO.Console.Input;
using NVs.Budget.Infrastructure.IO.Console.Input.Criteria;
using NVs.Budget.Infrastructure.IO.Console.Input.Criteria.Logbook;
using NVs.Budget.Infrastructure.IO.Console.Input.CsvOperationsReader;
using NVs.Budget.Infrastructure.IO.Console.Input.CsvTransfersReader;
using NVs.Budget.Infrastructure.IO.Console.Input.Options;
using NVs.Budget.Infrastructure.IO.Console.Options;
using NVs.Budget.Infrastructure.IO.Console.Output;
using NVs.Budget.Infrastructure.IO.Console.Output.Accounts;
using NVs.Budget.Infrastructure.IO.Console.Output.Logbook;
using NVs.Budget.Infrastructure.IO.Console.Output.Operations;
using NVs.Budget.Infrastructure.IO.Console.Output.Owners;
using NVs.Budget.Infrastructure.IO.Console.Output.Results;

[assembly:InternalsVisibleTo("NVs.Budget.Infrastructure.IO.Console.Tests")]
namespace NVs.Budget.Infrastructure.IO.Console;

public static class ConsoleIOExtensions
{
    public static IServiceCollection AddConsoleIO(this IServiceCollection services)
    {
        services.AddAutoMapper(c => c.AddProfile(new CsvMappingProfile()));
        services.AddTransient<IObjectWriter<TrackedOwner>, OwnersWriter>();
        services.AddTransient<IObjectWriter<TrackedOperation>, TrackedOperationsWriter>();
        services.AddTransient<IObjectWriter<TrackedTransfer>, TransfersWriter>();
        services.AddTransient<IObjectWriter<Operation>, OperationsWriter>();
        services.AddTransient<IObjectWriter<TrackedBudget>, TrackedAccountsWriter>();

        services.AddTransient(typeof(IResultWriter<>), typeof(GenericResultWriter<>));
        services.AddTransient<IResultWriter<Result<TrackedOwner>>, OwnerResultWriter>();
        services.AddTransient<ILogbookWriter, LogbookWriter>();
        services.AddTransient<ICriteriaParser, CriteriaParser>();

        services.AddSingleton<IOutputStreamProvider, ConsoleOutputStreams>();
        services.AddTransient<IOperationsReader, CsvOperationsReader>();
        services.AddTransient<ITransfersReader, CsvTransfersReader>();
        services.AddTransient<IOutputOptionsChanger, OutputOptionsChanger>();

        services.AddTransient<IInputStreamProvider, ConsoleInputStream>();
        
        return services;
    }

    public static IServiceCollection UseConsoleIO(this IServiceCollection services, IConfiguration configuration)
    {
        var cultureCode = configuration.GetValue<string>("CultureCode");
        var culture = cultureCode is null ? CultureInfo.CurrentCulture : CultureInfo.GetCultureInfo(cultureCode);


        services.Configure<OutputOptions>(configuration.GetSection(nameof(OutputOptions)).Bind);
        services.Configure<ConfigurationBasedCsvReadingOptions>(c => c.UpdateFromConfiguration(configuration, culture));

        services.AddSingleton(configuration);


        services.AddSingleton(new CsvConfiguration(culture)
        {
            IgnoreBlankLines = true,
            HeaderValidated = null,
            HasHeaderRecord = true,
            DetectDelimiter = true
        });

        var criteriaParser = new CriteriaParser();
        var substitutionsParser = new SubstitutionsParser(criteriaParser);
        var criteriaListReader = new CriteriaListReader(criteriaParser, substitutionsParser, configuration);

        var transferCriteria = criteriaListReader.GetTransferCriteria();
        services.AddSingleton(transferCriteria);

        var taggingCriteria = criteriaListReader.GetTaggingCriteria();
        services.AddSingleton(taggingCriteria);

        var logbookCriteriaReader = new YamlLogbookRulesetReader(criteriaParser, substitutionsParser);
        services.AddSingleton<ILogbookCriteriaReader>(logbookCriteriaReader);

        return services;
    }
}
