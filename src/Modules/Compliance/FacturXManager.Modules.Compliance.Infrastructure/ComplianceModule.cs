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

        // Rendu PDF/A-3b basé sur PdfSharp 6.x. Conserver UnconfiguredFacturXPdfA3BRenderer
        // pour les contextes où le rendu PDF n'est pas souhaité.
        services.AddScoped<IFacturXPdfA3BRenderer, FacturXPdfA3BRenderer>();
    }

    private static Task ConfigureAsync(this IServiceCollection services, IConfiguration configuration) =>
        Task.CompletedTask;
}
