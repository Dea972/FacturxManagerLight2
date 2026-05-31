using FacturXManager.Common.Domain;
using FacturXManager.Common.Presentation.ApiResults;
using FacturXManager.Common.Presentation.Endpoints;
using FacturXManager.Modules.Compliance.Application.Abstractions;
using FacturXManager.Modules.Compliance.Application.Invoicing.ExtractPdfMetadata;
using FacturXManager.Modules.Compliance.Application.Invoicing.InspectInvoiceMetadata;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace FacturXManager.Modules.Compliance.Presentation.Invoicing;

internal sealed class InspectInvoiceMetadata : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("compliance/invoices/metadata", async (Request request, ISender sender) =>
        {
            Result<byte[]> result = await sender.Send(new InspectInvoiceMetadataCommand(request.Invoice, request.MetadataType));
            return result.Match(
                bytes => request.MetadataType switch
                {
                    PdfMetadataType.Zip => Results.File(bytes, "application/zip", $"facturx-metadata.zip"),
                    _ => Results.Text(System.Text.Encoding.UTF8.GetString(bytes), "application/xml")
                },
                ApiResults.Problem);
        })
        .WithTags(Tags.Compliance)
        .WithSummary("Inspecter les métadonnées d'une facture Factur-X générée")
        .WithDescription(
            "Génère le PDF Factur-X à partir du modèle de facture et retourne, selon le type demandé : " +
            "le XMP brut du catalogue PDF (Xmp), le XML CII embarqué factur-x.xml (EmbeddedXml), " +
            "ou une archive ZIP contenant le PDF, le XML CII et le XMP (Zip).")
        .Produces<string>(contentType: "application/xml")
        .Produces<byte[]>(contentType: "application/zip")
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);
    }

    internal sealed record Request(InvoiceModel Invoice, PdfMetadataType MetadataType);
}
