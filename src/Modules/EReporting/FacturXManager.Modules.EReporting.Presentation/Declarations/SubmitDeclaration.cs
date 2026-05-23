using FacturXManager.Common.Domain;
using FacturXManager.Common.Presentation.ApiResults;
using FacturXManager.Common.Presentation.Endpoints;
using FacturXManager.Modules.EReporting.Application.Declarations.SubmitDeclaration;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace FacturXManager.Modules.EReporting.Presentation.Declarations;

internal sealed class SubmitDeclaration : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("ereporting/declarations/{declarationId:guid}/submit",
            async (Guid declarationId, ISender sender) =>
            {
                Result<SubmitDeclarationSummary> result =
                    await sender.Send(new SubmitDeclarationCommand(declarationId));

                return result.Match(Results.Ok, ApiResults.Problem);
            })
        .WithTags(Tags.EReporting)
        .WithDescription("Soumet une déclaration finalisée à la DGFiP.")
        .Produces<SubmitDeclarationSummary>()
        .Produces<ProblemDetails>(StatusCodes.Status409Conflict);
    }
}
