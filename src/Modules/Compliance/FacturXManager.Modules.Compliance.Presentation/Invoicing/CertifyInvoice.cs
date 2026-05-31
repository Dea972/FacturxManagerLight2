using FacturXManager.Common.Domain;
using FacturXManager.Common.Presentation.ApiResults;
using FacturXManager.Common.Presentation.Endpoints;
using FacturXManager.Modules.Compliance.Application.Abstractions;
using FacturXManager.Modules.Compliance.Application.Invoicing.CertifyInvoice;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace FacturXManager.Modules.Compliance.Presentation.Invoicing;

internal sealed class CertifyInvoice : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("compliance/invoices/certify", async (Request request, ISender sender) =>
        {
            Result<CertifyInvoiceSummary> result =
                await sender.Send(new CertifyInvoiceCommand(request.Invoice));

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .WithTags(Tags.Compliance)
        .WithSummary("Certifier une facture Factur-X")
        .WithDescription(
            "Certifie une facture : validation EN16931, génération du XML CII et du PDF/A-3b embarquant factur-x.xml. " +
            "Retourne le document certifié encodé en Base64. Profils supportés : EN16931 (Comfort), Extended.")
        .Produces<CertifyInvoiceSummary>()
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);
    }

    internal sealed record Request(InvoiceModel Invoice);
}
