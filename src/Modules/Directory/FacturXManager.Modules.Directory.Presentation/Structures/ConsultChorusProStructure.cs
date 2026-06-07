using FacturXManager.Common.Domain;
using FacturXManager.Common.Presentation.ApiResults;
using FacturXManager.Common.Presentation.Endpoints;
using FacturXManager.Modules.Directory.Application.Structures.ConsultChorusProStructure;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace FacturXManager.Modules.Directory.Presentation.Structures;

internal sealed class ConsultChorusProStructure : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("directory/chorus-pro/structures/{idStructureCpp:long}", async (long idStructureCpp, ISender sender) =>
        {
            Result<ChorusProStructureDetailResponse> result =
                await sender.Send(new ConsultChorusProStructureQuery(idStructureCpp));

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .WithTags(Tags.Directory)
        .WithSummary("Consulter le détail d'une structure Chorus Pro")
        .WithDescription(
            "Interroge l'API PISTE /structures/v1/consulter pour récupérer le détail complet d'une structure " +
            "Chorus Pro : coordonnées, services associés, identifiant technique.")
        .Produces<ChorusProStructureDetailResponse>()
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
    }
}
