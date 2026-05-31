using FacturXManager.Common.Domain;
using FacturXManager.Common.Presentation.ApiResults;
using FacturXManager.Common.Presentation.Endpoints;
using FacturXManager.Modules.Compliance.Application.Invoicing.ExtractPdfMetadata;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace FacturXManager.Modules.Compliance.Presentation.Invoicing;

internal sealed class ExtractPdfMetadata : IEndpoint{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("compliance/invoices/extract-metadata", async (
                [FromQuery] PdfMetadataType type,
                IFormFile file,
                ISender sender) =>
        {
            if (file.Length == 0)
            {
                return Results.BadRequest("Le fichier PDF est vide.");
            }

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            byte[] pdfBytes = ms.ToArray();

            Result<ExtractPdfMetadataResponse> result =
                await sender.Send(new ExtractPdfMetadataQuery(pdfBytes, type));

            return result.Match(r => Results.Ok(r.Content), ApiResults.Problem);
        })
        .WithTags(Tags.Invoice)
        .WithSummary("Extraire les métadonnées d'un PDF Factur-X")
        .WithDescription(
            "Upload un PDF Factur-X et retourne ses métadonnées en texte brut. " +
            "Type=Xmp retourne le XMP du catalogue (pdfaid, fx:, Dublin Core). " +
            "Type=EmbeddedXml retourne le XML CII embarqué (factur-x.xml).")
        .Produces<string>()
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .DisableAntiforgery();
    }
}
