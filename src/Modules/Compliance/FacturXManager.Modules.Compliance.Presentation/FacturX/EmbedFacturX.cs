using System.Text.Json;
using FacturXManager.Common.Domain;
using FacturXManager.Common.Presentation.ApiResults;
using FacturXManager.Common.Presentation.Endpoints;
using FacturXManager.Modules.Compliance.Application.Abstractions.Invoices;
using FacturXManager.Modules.Compliance.Application.Invoicing.EmbedFacturX;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace FacturXManager.Modules.Compliance.Presentation.FacturX;

internal sealed class EmbedFacturX : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("compliance/invoices/embed", async (
            IFormFile file,
            [FromForm] string invoice,
            ISender sender) =>
        {
            if (file == null || file.Length == 0)
            {
                return Results.BadRequest("Le fichier PDF d'origine est requis et ne peut pas être vide.");
            }

            if (string.IsNullOrWhiteSpace(invoice))
            {
                return Results.BadRequest("Le modèle de données de facture (invoice) sous forme de formulaire JSON est requis.");
            }

            InvoiceModel? invoiceModel;
            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                invoiceModel = JsonSerializer.Deserialize<InvoiceModel>(invoice, options);
            }
            catch (JsonException)
            {
                return Results.BadRequest("Les données de facturation (invoice) fournies ne sont pas un JSON valide pour un InvoiceModel.");
            }

            if (invoiceModel == null)
            {
                return Results.BadRequest("Impossible de désérialiser les données de facturation.");
            }

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            byte[] pdfBytes = ms.ToArray();

            Result<byte[]> result = await sender.Send(new EmbedFacturXCommand(pdfBytes, invoiceModel));

            return result.Match(
                bytes => Results.File(bytes, "application/pdf", $"facturx-{invoiceModel.Number}.pdf"),
                ApiResults.Problem);
        })
        .WithTags(Tags.Invoice)
        .WithSummary("Embarquer Factur-X dans un PDF existant")
        .WithDescription(
            "Reçoit un PDF visuel existant et les données de facture (InvoiceModel) en form-data, " +
            "génère le XML CII conforme EN16931 et l'embarque dans le PDF fourni pour produire un PDF/A-3b Factur-X. " +
            "Retourne directement le PDF enrichi en téléchargement.")
        .Produces(StatusCodes.Status200OK, contentType: "application/pdf")
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .DisableAntiforgery();
    }
}
