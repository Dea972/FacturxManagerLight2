using FacturXManager.Common.Domain;
using FacturXManager.Common.Presentation.ApiResults;
using FacturXManager.Common.Presentation.Endpoints;
using FacturXManager.Modules.EReporting.Application.Declarations.FinalizeDeclaration;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FacturXManager.Modules.EReporting.Presentation.Declarations;

internal sealed class FinalizeDeclaration : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("ereporting/declarations/{declarationId:guid}/finalize",
            async (Guid declarationId, ISender sender) =>
            {
                Result result = await sender.Send(new FinalizeDeclarationCommand(declarationId));

                return result.Match(Results.NoContent, ApiResults.Problem);
            })
        .WithTags(Tags.EReporting)
        .WithDescription("Finalise une déclaration avant soumission.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ProblemDetails>(StatusCodes.Status409Conflict);
    }
}
