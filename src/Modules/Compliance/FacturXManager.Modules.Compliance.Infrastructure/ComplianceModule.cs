using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FacturXManager.Common.Presentation.Endpoints;
using FacturXManager.Modules.Compliance.Application.Abstractions;
using FacturXManager.Modules.Compliance.Infrastructure.Invoicing;

#pragma warning disable S1172 // Unused method parameters should be removed
namespace FacturXManager.Modules.Compliance.Infrastructure;

public static class ComplianceModule
{
    public static async Task<IServiceCollection> AddComplianceModuleAsync(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMediatR(config =>
            config.RegisterServicesFromAssembly(Application.AssemblyReference.Assembly));

        services.AddValidatorsFromAssembly(
            Application.AssemblyReference.Assembly,
            includeInternalTypes: true);

        services.AddInfrastructure();

        services.AddEndpoints(Presentation.AssemblyReference.Assembly);

        await services.ConfigureAsync(configuration);

        return services;
    }

    private static void AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IInvoiceBuilder, InvoiceBuilder>();
        services.AddScoped<IEn16931Validator, En16931Validator>();
        services.AddScoped<ICrossIndustryInvoiceWriter, CrossIndustryInvoiceWriter>();

        // Point d'extension PDF/A-3b : remplacer par une implémentation adossée à une bibliothèque PDF.
        services.AddScoped<IFacturXPdfA3bRenderer, UnconfiguredFacturXPdfA3bRenderer>();
    }

    private static Task ConfigureAsync(this IServiceCollection services, IConfiguration configuration) =>
        Task.CompletedTask;
}
