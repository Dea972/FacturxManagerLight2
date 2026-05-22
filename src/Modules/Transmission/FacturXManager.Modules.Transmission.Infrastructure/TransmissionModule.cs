using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FacturXManager.Common.Presentation.Endpoints;

#pragma warning disable S1172 // Unused method parameters should be removed
namespace FacturXManager.Modules.Transmission.Infrastructure;

public static class TransmissionModule
{
    public static async Task<IServiceCollection> AddTransmissionModuleAsync(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMediatR(config => config.RegisterServicesFromAssembly(Assembly.GetAssembly(typeof(TransmissionModule))!));

        services.AddInfrastructure();

        services.AddEndpoints(Presentation.AssemblyReference.Assembly);

        await services.ConfigureAsync(configuration);

        return services;
    }

    private static void AddInfrastructure(this IServiceCollection services)
    {
        // Register infrastructure services here
        // Example:
        // services.AddScoped<IRepository, Repository>();
    }

    private static Task ConfigureAsync(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure options here
        // Example:
        // services.Configure<TransmissionOptions>(configuration.GetSection("Transmission"));

        return Task.CompletedTask;
    }
}
