using Microsoft.Extensions.DependencyInjection;
using NVs.Budget.Application.UseCases.Owners;

namespace NVs.Budget.Application.UseCases;

public static class ApplicationLogicExtensions
{
    public static IServiceCollection AddApplicationUseCases(this IServiceCollection services)
    {
        services.AddMediatR(c => c.RegisterServicesFromAssemblyContaining<RegisterOwnerHandler>());

        return services;
    }
}
