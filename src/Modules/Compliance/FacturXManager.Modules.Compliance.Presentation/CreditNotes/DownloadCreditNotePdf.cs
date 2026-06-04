using FacturXManager.Common.Domain;
using FacturXManager.Common.Presentation.ApiResults;
using FacturXManager.Common.Presentation.Endpoints;
using FacturXManager.Modules.Compliance.Application.Abstractions.CreditNotes;
using FacturXManager.Modules.Compliance.Application.CreditNotes.DownloadCreditNotePdf;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace FacturXManager.Modules.Compliance.Presentation.CreditNotes;

internal sealed class DownloadCreditNotePdf : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("compliance/credit-notes/download", async (CreditNoteRequest request, ISender sender) =>
        {
            Result<DownloadCreditNotePdfSummary> result =
                await sender.Send(new DownloadCreditNotePdfCommand(request.CreditNote));

            return result.Match(
                summary => Results.File(summary.PdfBytes, "application/pdf", summary.FileName),
                ApiResults.Problem);
        })
        .WithTags(Tags.CreditNote)
        .WithSummary("Télécharger le PDF Factur-X d'un avoir")
        .WithDescription(
            "Génère et télécharge directement le PDF/A-3b Factur-X de l'avoir (type CII 381, retour binaire application/pdf). " +
            "Le fichier factur-x.xml est embarqué dans le PDF avec AFRelationship=Data.")
        .Produces(StatusCodes.Status200OK, contentType: "application/pdf")
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);
    }

    internal sealed record CreditNoteRequest(CreditNoteModel CreditNote);
}
