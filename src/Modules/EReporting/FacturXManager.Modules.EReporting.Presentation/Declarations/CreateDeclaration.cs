using FacturXManager.Common.Domain;
using FacturXManager.Common.Presentation.ApiResults;
using FacturXManager.Common.Presentation.Endpoints;
using FacturXManager.Modules.EReporting.Application.Declarations.CreateDeclaration;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace FacturXManager.Modules.EReporting.Presentation.Declarations;

internal sealed class CreateDeclaration : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("ereporting/declarations", async (Request request, ISender sender) =>
        {
            Result<Guid> result = await sender.Send(new CreateDeclarationCommand(
                request.Year,
                request.Month,
                request.TransactionType));

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .WithTags(Tags.EReporting)
        .WithDescription("Crée une déclaration d'e-reporting au statut Brouillon.")
        .Produces<Guid>()
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);
    }

    internal sealed record Request(int Year, int Month, string TransactionType);
}
