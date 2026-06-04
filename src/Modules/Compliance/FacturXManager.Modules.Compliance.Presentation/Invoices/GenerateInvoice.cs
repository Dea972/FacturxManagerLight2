using FacturXManager.Common.Domain;
using FacturXManager.Common.Presentation.ApiResults;
using FacturXManager.Common.Presentation.Endpoints;
using FacturXManager.Modules.Compliance.Application.Abstractions.Invoices;
using FacturXManager.Modules.Compliance.Application.Invoices.GenerateInvoice;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace FacturXManager.Modules.Compliance.Presentation.Invoices;

internal sealed class GenerateInvoice : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("compliance/invoices/generate", async (Request request, ISender sender) =>
        {
            Result<GenerateInvoiceSummary> result =
                await sender.Send(new GenerateInvoiceCommand(request.Invoice));

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .WithTags(Tags.Invoice)
        .WithSummary("Générer une facture Factur-X")
        .WithDescription(
            "Génère une facture : validation EN16931, génération du XML CII et du PDF/A-3b embarquant factur-x.xml. " +
            "Retourne le document généré encodé en Base64. Profils supportés : EN16931 (Comfort), Extended.")
        .Produces<GenerateInvoiceSummary>()
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);
    }

    internal sealed record Request(InvoiceModel Invoice);
}
