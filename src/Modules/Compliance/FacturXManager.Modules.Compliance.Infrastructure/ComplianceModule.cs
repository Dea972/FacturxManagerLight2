using FacturXManager.Common.Presentation.Endpoints;
using FacturXManager.Modules.Compliance.Application;
using FacturXManager.Modules.Compliance.Application.Abstractions;
using FacturXManager.Modules.Compliance.Application.Abstractions.Data;
using FacturXManager.Modules.Compliance.Domain.Common;
using FacturXManager.Modules.Compliance.Domain.CreditNotes;
using FacturXManager.Modules.Compliance.Infrastructure.CreditNotes;
using FacturXManager.Modules.Compliance.Infrastructure.Database;
using FacturXManager.Modules.Compliance.Infrastructure.FacturX;
using FacturXManager.Modules.Compliance.Infrastructure.Invoices;
using FacturXManager.Modules.Compliance.Infrastructure.Invoicing;
using FacturXManager.Modules.Compliance.Infrastructure.PdfSharp;
using FacturXManager.Modules.Compliance.Infrastructure.QuestPdf;
using FacturXManager.Modules.Compliance.Infrastructure.Zugferd;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuestPDF.Infrastructure;

#pragma warning disable S1172 // Unused method parameters should be removed
namespace FacturXManager.Modules.Compliance.Infrastructure;

public static class ComplianceModule
{
    public static async Task<IServiceCollection> AddComplianceModuleAsync(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddComplianceApplication();

        services.AddInfrastructure(configuration);

        services.AddEndpoints(Presentation.AssemblyReference.Assembly);

        await services.ConfigureAsync(configuration);

        return services;
    }

    private static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("Database") ?? string.Empty;

        services.AddDbContext<ComplianceDbContext>(options =>
            options
                .UseSqlite(connectionString, sql => sql.MigrationsHistoryTable("__EFMigrationsHistory"))
                .UseSnakeCaseNamingConvention());

        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<ICreditNoteRepository, CreditNoteRepository>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ComplianceDbContext>());
        QuestPDF.Settings.EnableDebugging = true;

        services.AddScoped<ICrossIndustryInvoiceWriter, ZugferdCrossIndustryInvoiceWriter>();
        services.AddScoped<IFacturXMetadataExtractor, FacturXMetadataExtractor>();
        services.AddScoped<IFacturXPdfEmbedder, QuestPdfFacturXEmbedder>();

        // Keyed: implémentation alternative XML CII (System.Xml.Linq, sans dépendance)
        services.AddKeyedScoped<ICrossIndustryInvoiceWriter, CrossIndustryInvoiceWriter>("xmllinq");

        // Rendu PDF/A-3b via QuestPDF (par défaut) — PDF/A natif, attachments, XMP Factur-X
        services.AddScoped<IFacturXPdfA3BRenderer, QuestPdfFacturXA3BRenderer>();

        // Keyed: rendu PDF/A-3b via PdfSharp (fallback, post-traitement binaire XMP)
        services.AddKeyedScoped<IFacturXPdfA3BRenderer, PdfSharpFacturXPdfA3BRenderer>("pdfsharp");
    }

    private static Task ConfigureAsync(this IServiceCollection services, IConfiguration configuration)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        return Task.CompletedTask;
    }
}
