using System.Text.Json;
using FacturXManager.Common.Domain;
using FacturXManager.Common.Presentation.ApiResults;
using FacturXManager.Common.Presentation.Endpoints;
using FacturXManager.Modules.Compliance.Application.Abstractions.Invoices;
using FacturXManager.Modules.Compliance.Application.FacturX.AttachFacturX;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace FacturXManager.Modules.Compliance.Presentation.Invoicing;

internal sealed class AttachFacturX : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("compliance/invoices/attach-facturx", async (
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
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
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

            Result<AttachFacturXSummary> result =
                await sender.Send(new AttachFacturXCommand(pdfBytes, invoiceModel));

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .WithTags(Tags.Invoice)
        .WithSummary("Attacher Factur-X à un PDF existant")
        .WithDescription(
            "Prend un fichier PDF visuel et ses métadonnées métier (InvoiceModel) sous forme de formulaire, " +
            "génère le XML CII conforme EN16931 et l'intègre au sein du PDF existant pour produire un PDF/A-3b Factur-X.")
        .Produces<AttachFacturXSummary>()
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .DisableAntiforgery();

    }
}
