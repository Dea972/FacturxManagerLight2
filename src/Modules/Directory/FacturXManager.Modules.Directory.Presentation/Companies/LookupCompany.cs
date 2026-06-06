using FacturXManager.Common.Domain;
using FacturXManager.Common.Presentation.ApiResults;
using FacturXManager.Common.Presentation.Endpoints;
using FacturXManager.Modules.Directory.Application.Companies.LookupCompany;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace FacturXManager.Modules.Directory.Presentation.Companies;

internal sealed class LookupCompany : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("directory/companies/{identifier}", async (string identifier, ISender sender) =>
        {
            Result<CompanyLookupResponse> result = await sender.Send(new LookupCompanyQuery(identifier));

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .WithTags(Tags.Directory)
        .WithSummary("Rechercher une entreprise (SIREN/SIRET)")
        .WithDescription(
            "Recherche l'identité d'une entreprise française à partir d'un SIREN (9 chiffres) ou d'un SIRET (14 chiffres). " +
            "Source : API SIRENE (INSEE). Retourne la raison sociale, le statut administratif et la diffusabilité RGPD.")
        .Produces<CompanyLookupResponse>()
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
    }
}
