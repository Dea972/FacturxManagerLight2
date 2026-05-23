using FacturXManager.Common.Domain;
using FacturXManager.Common.Presentation.ApiResults;
using FacturXManager.Common.Presentation.Endpoints;
using FacturXManager.Modules.EReporting.Application.Declarations.GetDeclaration;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace FacturXManager.Modules.EReporting.Presentation.Declarations;

internal sealed class GetDeclaration : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("ereporting/declarations/{declarationId:guid}",
            async (Guid declarationId, ISender sender) =>
            {
                Result<DeclarationResponse> result = await sender.Send(new GetDeclarationQuery(declarationId));

                return result.Match(Results.Ok, ApiResults.Problem);
            })
        .WithTags(Tags.EReporting)
        .WithDescription("Retourne le détail et le statut d'une déclaration.")
        .Produces<DeclarationResponse>()
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
    }
}
