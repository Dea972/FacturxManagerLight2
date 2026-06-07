using FacturXManager.Common.Domain;
using FacturXManager.Common.Presentation.ApiResults;
using FacturXManager.Common.Presentation.Endpoints;
using FacturXManager.Modules.Directory.Application.Companies.LookupCompany;
using FacturXManager.Modules.Directory.Application.Structures.SearchChorusProStructure;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace FacturXManager.Modules.Directory.Presentation.Structures;

internal sealed class SearchChorusProStructure : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("directory/chorus-pro/structures/search", async ([AsParameters] Request req, ISender sender) =>
        {
            Result<CompanyLookupResponse> result =
                await sender.Send(new SearchChorusProStructureQuery(req.Siret));

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .WithTags(Tags.Directory)
        .WithSummary("Rechercher une structure Chorus Pro par SIRET")
        .WithDescription(
            "Interroge l'API PISTE /structures/v1/rechercher pour retrouver une structure Chorus Pro " +
            "et récupérer son identifiant technique de rattachement PPF. " +
            "Complète la consultation SIRENE en y ajoutant les informations de routage.")
        .Produces<CompanyLookupResponse>()
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
    }

    private sealed record Request([FromQuery] string Siret);
}
