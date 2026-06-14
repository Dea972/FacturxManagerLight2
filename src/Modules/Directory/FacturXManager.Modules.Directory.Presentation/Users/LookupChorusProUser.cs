using FacturXManager.Common.Domain;
using FacturXManager.Common.Presentation.ApiResults;
using FacturXManager.Common.Presentation.Endpoints;
using FacturXManager.Modules.Directory.Application.Users.LookupChorusProUser;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace FacturXManager.Modules.Directory.Presentation.Users;

internal sealed class LookupChorusProUser : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("directory/chorus-pro/users/{login}", async (string login, ISender sender) =>
        {
            Result<ChorusProUserLookupResponse> result =
                await sender.Send(new LookupChorusProUserQuery(login));

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .WithTags(Tags.Directory)
        .WithSummary("Consulter un utilisateur Chorus Pro")
        .WithDescription(
            "Consulte un utilisateur Chorus Pro à partir de son login (adresse e-mail ou identifiant technique). " +
            "Source : API PISTE /utilisateurs/v1/consulter. " +
            "⚠️ Requiert des droits de gestionnaire sur la structure Chorus Pro cible ; " +
            "les credentials de service (client_credentials) peuvent retourner une réponse limitée.")
        .Produces<ChorusProUserLookupResponse>()
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);
    }
}
