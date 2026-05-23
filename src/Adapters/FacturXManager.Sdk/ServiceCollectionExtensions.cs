using FacturXManager.Common.Application;
using FacturXManager.Common.Infrastructure;
using FacturXManager.Common.Presentation.Endpoints;
using FacturXManager.Modules.AiEngine.Infrastructure;
using FacturXManager.Modules.Compliance.Infrastructure;
using FacturXManager.Modules.EReporting.Infrastructure;
using FacturXManager.Modules.Transmission.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FacturXManager.Infrastructure;

/// <summary>
/// Provides extension methods to bootstrap the Modular Monolith Template.
/// Centralizes registration of application, infrastructure, tools, endpoints and modules.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all core services required by the modular monolith.
    /// Add your module-specific registration calls in the indicated section.
    /// </summary>
    /// <param name="services">The DI service collection.</param>
    /// <param name="connectionString">The database or persistence connection string.</param>
    /// <param name="configuration">The application configuration root.</param>
    /// <returns>The same IServiceCollection for fluent chaining.</returns>
    /// <remarks>
    /// Currently this method does not perform any awaited operations.
    /// The async signature is kept in case future module initialization requires asynchronous calls.
    /// </remarks>
    public async static Task<IServiceCollection> AddFacturXManagerAsync(
        this IServiceCollection services,
        string connectionString,
        IConfiguration configuration)
    {
        // Register application layer services (CQRS handlers, mediators, validators, etc.).
        services.AddApplication([
            Modules.Compliance.Application.AssemblyReference.Assembly, 
            Modules.EReporting.Application.AssemblyReference.Assembly,
            Modules.Transmission.Application.AssemblyReference.Assembly
        ]);

        // Register infrastructure concerns (DbContext, repositories, outbox, message bus, etc.).
        services.AddInfrastructure(connectionString, []);



        // Modules métier Factur-X (réforme française de facturation électronique).
        await services.AddComplianceModuleAsync(configuration);
        await services.AddTransmissionModuleAsync(configuration);
        await services.AddEReportingModuleAsync(configuration);


        // Les endpoints de chaque module sont déjà enregistrés par leur Add<Module>ModuleAsync respectif.

        // Return the modified service collection.
        return services;
    }

    /// <summary>
    /// Maps all collected endpoints under the /api route group.
    /// Call this in Program.cs after building the WebApplication.
    /// </summary>
    /// <param name="app">The current WebApplication instance.</param>
    /// <returns>The same IApplicationBuilder for fluent chaining.</returns>
    public static IApplicationBuilder MapFacturXManagerEndpoints(this WebApplication app)
    {
        // Create a route group for version-neutral API endpoints.
        RouteGroupBuilder apiGroup = app.MapGroup("/api");

        // Delegate mapping to the endpoint discovery/registration mechanism.
        app.MapEndpoints(apiGroup);

        // Return the application builder for further middleware chaining.
        return app;
    }
}
