using FacturXManager.Common.Domain;
using FacturXManager.Common.Presentation.ApiResults;
using FacturXManager.Common.Presentation.Endpoints;
using FacturXManager.Modules.EReporting.Application.Declarations.AddDeclarationEntry;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace FacturXManager.Modules.EReporting.Presentation.Declarations;

internal sealed class AddDeclarationEntry : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("ereporting/declarations/{declarationId:guid}/entries",
            async (Guid declarationId, Request request, ISender sender) =>
            {
                Result result = await sender.Send(new AddDeclarationEntryCommand(
                    declarationId,
                    request.TaxableAmount,
                    request.VatAmount,
                    request.CountryCode,
                    request.OperationNature));

                return result.Match(Results.NoContent, ApiResults.Problem);
            })
        .WithTags(Tags.EReporting)
        .WithDescription("Ajoute une transaction à une déclaration au statut Brouillon.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);
    }

    internal sealed record Request(
        decimal TaxableAmount,
        decimal VatAmount,
        string CountryCode,
        string OperationNature);
}
