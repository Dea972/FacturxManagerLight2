using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FacturXManager.Common.Presentation.Endpoints;
using FacturXManager.Modules.Compliance.Application.Abstractions;
using FacturXManager.Modules.Compliance.Infrastructure.Invoicing;
using FacturXManager.Modules.Compliance.Infrastructure.QuestPdf;
using FacturXManager.Modules.Compliance.Infrastructure.Zugferd;

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
        services.AddScoped<ICrossIndustryInvoiceWriter, ZugferdCrossIndustryInvoiceWriter>();
        services.AddScoped<IFacturXMetadataExtractor, FacturXMetadataExtractor>();

        // Keyed: implémentation alternative XML CII (System.Xml.Linq, sans dépendance)
        services.AddKeyedScoped<ICrossIndustryInvoiceWriter, CrossIndustryInvoiceWriter>("xmllinq");

        // Rendu PDF/A-3b via QuestPDF (par défaut) — PDF/A natif, attachments, XMP Factur-X
        services.AddScoped<IFacturXPdfA3BRenderer, QuestPdfFacturXRenderer>();

        // Keyed: rendu PDF/A-3b via PdfSharp (fallback, post-traitement binaire XMP)
        services.AddKeyedScoped<IFacturXPdfA3BRenderer, FacturXPdfA3BRenderer>("pdfsharp");
    }

    private static Task ConfigureAsync(this IServiceCollection services, IConfiguration configuration) =>
        Task.CompletedTask;
}
