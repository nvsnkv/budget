using System.Reflection;
using System.Runtime.CompilerServices;
using Asp.Versioning;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using NVs.Budget.Controllers.Web.Filters;
using NVs.Budget.Controllers.Web.Formatters;
using NVs.Budget.Controllers.Web.Utils;
using Swashbuckle.AspNetCore.SwaggerGen;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

[assembly: InternalsVisibleTo("NVs.Budget.Controllers.Web.Tests")]

namespace NVs.Budget.Controllers.Web;

public static class WebControllersExtensions
{
    public static IServiceCollection AddWebControllers(this IServiceCollection services)
    {
        services.AddSingleton(new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build());

        
        services.AddScoped<BudgetMapper>();
        services.AddScoped<FileReadingSettingsMapper>();
        
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(o =>
        {
            o.SwaggerDoc("v0.1", new OpenApiInfo { Title = "Budget API", Version = "v0.1" });
            o.DocInclusionPredicate((docName, apiDesc) =>
            {
                if (!apiDesc.TryGetMethodInfo(out MethodInfo methodInfo))
                    return false;
                var versions = methodInfo.DeclaringType!
                    .GetCustomAttributes(true)
                    .OfType<ApiVersionAttribute>()
                    .SelectMany(attr => attr.Versions);
                var versionMatched = versions.Any(v => $"v{v}" == docName);
                if (versionMatched)
                {
                    if (apiDesc.RelativePath?.StartsWith("api/v{version}/") == true)
                    {
                        apiDesc.RelativePath = apiDesc.RelativePath.Replace("api/v{version}/", $"api/{docName}/");
                        var versionParam = apiDesc.ParameterDescriptions
                            .SingleOrDefault(p => p.Name == "version" && p.Source.Id == "Path");
                        if (versionParam != null)
                            apiDesc.ParameterDescriptions.Remove(versionParam);
                    }
                }

                return versionMatched;
            });
            
            // Configure lowercase paths
            o.DocumentFilter<LowercaseDocumentFilter>();
            
            // Configure enum string values
            o.UseInlineDefinitionsForEnums();
            o.SchemaFilter<EnumSchemaFilter>();
        });

        var assembly = typeof(WebControllersExtensions).Assembly;
        var part = new AssemblyPart(assembly);
        services
            .AddControllersWithViews(opts =>
            {
                var serializer = new SerializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build();
                
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .IgnoreUnmatchedProperties()
                    .Build();

                opts.OutputFormatters.Add(new YamlOutputFormatter(serializer));
                opts.InputFormatters.Insert(0, new YamlInputFormatter(deserializer));
                opts.FormatterMappings.SetMediaTypeMappingForFormat("yaml", "application/yaml");
                
                // Add model state validation filter to return 400 on invalid input
                opts.Filters.Add<ValidateModelStateFilter>();
            })
            .ConfigureApplicationPartManager(apm => apm.ApplicationParts.Add(part));

        services.AddApiVersioning();


        return services;
    }

    public static WebApplication UseWebControllers(this WebApplication app, bool useSwagger)
    {
        if (useSwagger)
        {
            app.UseSwagger();
            app.UseSwaggerUI(o => o.SwaggerEndpoint("/swagger/v0.1/swagger.json", "Budget API v0.1"));
        }
        
        return app;
    }
}
