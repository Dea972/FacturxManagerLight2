using FacturXManager.Common.Domain;
using FacturXManager.Common.Presentation.ApiResults;
using FacturXManager.Common.Presentation.Endpoints;
using FacturXManager.Modules.Transmission.Application.Transmissions.GetTransmission;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FacturXManager.Modules.Transmission.Presentation.Transmissions;

internal sealed class GetTransmission : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("transmissions/{transmissionId:guid}", async (Guid transmissionId, ISender sender) =>
        {
            Result<TransmissionResponse> result = await sender.Send(new GetTransmissionQuery(transmissionId));

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .WithTags(Tags.Transmission)
        .WithDescription("Retourne le statut et l'historique des tentatives d'une transmission.")
        .Produces<TransmissionResponse>()
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
    }
}
