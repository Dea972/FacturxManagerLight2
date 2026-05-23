using FacturXManager.Common.Domain;
using FacturXManager.Common.Presentation.ApiResults;
using FacturXManager.Common.Presentation.Endpoints;
using FacturXManager.Modules.Transmission.Application.Transmissions.SubmitTransmission;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace FacturXManager.Modules.Transmission.Presentation.Transmissions;

internal sealed class SubmitTransmission : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("transmissions", async (Request request, ISender sender) =>
        {
            Result<SubmitTransmissionSummary> result = await sender.Send(new SubmitTransmissionCommand(
                request.DocumentReference,
                request.RecipientSiret,
                request.Channel,
                request.FacturXPdfBase64,
                request.ExecutingService));

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .WithTags(Tags.Transmission)
        .WithDescription("Dépose un document certifié sur la plateforme du canal demandé et trace la tentative.")
        .Produces<SubmitTransmissionSummary>()
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);
    }

    internal sealed record Request(
        string DocumentReference,
        string RecipientSiret,
        string Channel,
        string FacturXPdfBase64,
        string? ExecutingService);
}
