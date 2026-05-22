using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FacturXManager.Common.Presentation.Endpoints;

#pragma warning disable S1172 // Unused method parameters should be removed
namespace FacturXManager.Modules.EReporting.Infrastructure;

public static class EReportingModule
{
    public static async Task<IServiceCollection> AddEReportingModuleAsync(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMediatR(config => config.RegisterServicesFromAssembly(Assembly.GetAssembly(typeof(EReportingModule))!));

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
        // services.Configure<EReportingOptions>(configuration.GetSection("EReporting"));

        return Task.CompletedTask;
    }
}
