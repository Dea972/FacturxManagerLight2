using FacturXManager.Common.Domain;
using FacturXManager.Common.Presentation.ApiResults;
using FacturXManager.Common.Presentation.Endpoints;
using FacturXManager.Modules.Compliance.Application.Abstractions;
using FacturXManager.Modules.Compliance.Application.Invoicing.GenerateCreditNote;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace FacturXManager.Modules.Compliance.Presentation.Invoicing;

internal sealed class GenerateCreditNote : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("compliance/credit-notes/generate", async (Request request, ISender sender) =>
        {
            Result<GenerateCreditNoteSummary> result =
                await sender.Send(new GenerateCreditNoteCommand(request.CreditNote));

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .WithTags(Tags.CreditNote)
        .WithSummary("Générer un avoir Factur-X")
        .WithDescription(
            "Génère un avoir Factur-X (type CII 381) : construction du XML CII avec InvoiceReferencedDocument et du PDF/A-3b. " +
            "Retourne le document encodé en Base64.")
        .Produces<GenerateCreditNoteSummary>()
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);
    }

    internal sealed record Request(CreditNoteModel CreditNote);
}
