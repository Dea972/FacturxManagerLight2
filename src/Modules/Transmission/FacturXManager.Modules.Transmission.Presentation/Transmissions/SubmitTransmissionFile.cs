using FacturXManager.Common.Domain;
using FacturXManager.Common.Presentation.ApiResults;
using FacturXManager.Common.Presentation.Endpoints;
using FacturXManager.Modules.Transmission.Application.Transmissions.SubmitTransmission;
using FacturXManager.Modules.Transmission.Application.Transmissions.SubmitTransmissionFile;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace FacturXManager.Modules.Transmission.Presentation.Transmissions;

internal sealed class SubmitTransmissionFile : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("transmissions/file", async (
            IFormFile file,
            [FromQuery] string documentReference,
            [FromQuery] string recipientSiret,
            [FromQuery] string channel,
            [FromQuery] string? executingService,
            ISender sender) =>
        {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            byte[] fileBytes = memoryStream.ToArray();

            Result<SubmitTransmissionSummary> result = await sender.Send(new SubmitTransmissionFileCommand(
                documentReference,
                recipientSiret,
                channel,
                fileBytes,
                executingService));

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .WithTags(Tags.Transmission)
        .WithSummary("Déposer une facture (fichier PDF)")
        .WithDescription(
            "Dépose un fichier PDF Factur-X directement (multipart/form-data) sur la plateforme du canal demandé. " +
            "Le fichier est converti en Base64 côté serveur avant transmission via DEPOT_PDF_API / PISTE.")
        .Produces<SubmitTransmissionSummary>()
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .DisableAntiforgery();
    }
}
