using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FacturXManager.Common.Presentation.Endpoints;
using FacturXManager.Modules.EReporting.Application.Abstractions;
using FacturXManager.Modules.EReporting.Application.Abstractions.Data;
using FacturXManager.Modules.EReporting.Domain.Declarations;
using FacturXManager.Modules.EReporting.Infrastructure.Database;
using FacturXManager.Modules.EReporting.Infrastructure.Declarations;
using FacturXManager.Modules.EReporting.Infrastructure.Dgfip;

namespace FacturXManager.Modules.EReporting.Infrastructure;

public static class EReportingModule
{
    public static async Task<IServiceCollection> AddEReportingModuleAsync(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMediatR(config =>
            config.RegisterServicesFromAssembly(Application.AssemblyReference.Assembly));

        services.AddValidatorsFromAssembly(
            Application.AssemblyReference.Assembly,
            includeInternalTypes: true);

        services.AddInfrastructure(configuration);

        services.AddEndpoints(Presentation.AssemblyReference.Assembly);

        await services.ConfigureAsync(configuration);

        return services;
    }

    private static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("Database") ?? string.Empty;

        services.AddDbContext<EReportingDbContext>(options =>
            options
                .UseSqlServer(connectionString, sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", Schemas.EReporting))
                .UseSnakeCaseNamingConvention());

        services.AddScoped<IDeclarationRepository, DeclarationRepository>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<EReportingDbContext>());

        // Point d'extension DGFiP : remplacer par un client opérationnel dès stabilisation de l'API.
        services.AddScoped<IDgfipReportingClient, UnconfiguredDgfipReportingClient>();
    }

    private static Task ConfigureAsync(this IServiceCollection services, IConfiguration configuration) =>
        Task.CompletedTask;
}
