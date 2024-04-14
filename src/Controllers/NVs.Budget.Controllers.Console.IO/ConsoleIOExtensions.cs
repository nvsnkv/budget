using System.Globalization;
using System.Runtime.CompilerServices;
using CsvHelper.Configuration;
using FluentResults;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Controllers.Console.Contracts.IO.Input;
using NVs.Budget.Controllers.Console.Contracts.IO.Output;
using NVs.Budget.Controllers.Console.IO.Input.CsvOperationsReader;
using NVs.Budget.Controllers.Console.IO.Input.Options;
using NVs.Budget.Controllers.Console.IO.Output;
using NVs.Budget.Controllers.Console.IO.Output.Operations;
using NVs.Budget.Controllers.Console.IO.Output.Owners;
using NVs.Budget.Controllers.Console.IO.Output.Results;
using NVs.Budget.Domain.Entities.Operations;

[assembly:InternalsVisibleTo("NVs.Budget.Controllers.Console.IO.Tests")]
namespace NVs.Budget.Controllers.Console.IO;

public static class ConsoleIOExtensions
{
    public static IServiceCollection AddConsoleIO(this IServiceCollection services)
    {
        services.AddAutoMapper(c => c.AddProfile(new CsvMappingProfile()));
        services.AddTransient<IObjectWriter<TrackedOwner>, OwnersWriter>();
        services.AddTransient<IObjectWriter<TrackedOperation>, TrackedOperationsWriter>();
        services.AddTransient<IObjectWriter<TrackedTransfer>, TransfersWriter>();
        services.AddTransient<IObjectWriter<Operation>, OperationsWriter>();

        services.AddTransient(typeof(IResultWriter<>), typeof(GenericResultWriter<>));
        services.AddTransient<IResultWriter<Result<TrackedOwner>>, OwnerResultWriter>();

        services.AddSingleton<IOutputStreamProvider, ConsoleOutputStreams>();
        services.AddTransient<IOperationsReader, CsvOperationsReader>();

        return services;
    }

    public static IServiceCollection UseConsoleIO(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<OutputOptions>(configuration.GetSection(nameof(OutputOptions)).Bind);
        services.Configure<CsvReadingOptions>(c => c.UpdateFromConfiguration(configuration));

        var cultureCode = configuration.GetValue<string>("CultureCode");
        var culture = cultureCode is null ? CultureInfo.CurrentCulture : CultureInfo.GetCultureInfo(cultureCode);

        services.AddSingleton(new CsvConfiguration(culture)
        {
            IgnoreBlankLines = true,
            HeaderValidated = null
        });
        return services;
    }
}
