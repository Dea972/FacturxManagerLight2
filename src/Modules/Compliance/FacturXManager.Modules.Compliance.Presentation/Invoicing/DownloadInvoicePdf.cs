using FacturXManager.Common.Domain;
using FacturXManager.Common.Presentation.ApiResults;
using FacturXManager.Common.Presentation.Endpoints;
using FacturXManager.Modules.Compliance.Application.Abstractions;
using FacturXManager.Modules.Compliance.Application.Invoicing.DownloadInvoicePdf;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace FacturXManager.Modules.Compliance.Presentation.Invoicing;

internal sealed class DownloadInvoicePdf : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("compliance/invoices/download", async (Request request, ISender sender) =>
        {
            Result<DownloadInvoicePdfSummary> result =
                await sender.Send(new DownloadInvoicePdfCommand(request.Invoice));

            return result.Match(
                summary => Results.File(summary.PdfBytes, "application/pdf", summary.FileName),
                ApiResults.Problem);
        })
        .WithTags(Tags.Invoice)
        .WithSummary("Télécharger le PDF Factur-X")
        .WithDescription(
            "Génère et télécharge directement le PDF/A-3b Factur-X de la facture (retour binaire application/pdf). " +
            "Le fichier factur-x.xml est embarqué dans le PDF avec AFRelationship=Data.")
        .Produces(StatusCodes.Status200OK, contentType: "application/pdf")
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);
    }

    internal sealed record Request(InvoiceModel Invoice);
}
