using System;
using System.Collections.Generic;
using System.Text;
using FacturXManager.Modules.Compliance.Application.Abstractions;
using FacturXManager.Modules.Compliance.Application.Abstractions.CreditNotes;
using FacturXManager.Modules.Compliance.Application.Abstractions.Invoices;
using FacturXManager.Modules.Compliance.Application.CreditNotes;
using FacturXManager.Modules.Compliance.Application.Invoices;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace FacturXManager.Modules.Compliance.Application;

public static class ApplicationConfiguration 
{
    public static IServiceCollection AddComplianceApplication(this IServiceCollection services)
    {
        services.AddMediatR(config =>
            config.RegisterServicesFromAssembly(Application.AssemblyReference.Assembly));

        services.AddValidatorsFromAssembly(
            Application.AssemblyReference.Assembly,
            includeInternalTypes: true);

        services.AddScoped<ICreditNoteBuilder, CreditNoteBuilder>();
        services.AddScoped<ICreditNoteService, CreditNoteService>();
        services.AddScoped<IInvoiceBuilder, InvoiceBuilder>();
        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddScoped<IEn16931Validator, En16931Validator>();
        return services;
    }
}
