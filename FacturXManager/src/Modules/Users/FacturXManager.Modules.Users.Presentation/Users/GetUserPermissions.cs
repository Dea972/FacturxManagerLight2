using FacturXManager.Common.Application.Authorization;
using FacturXManager.Common.Domain;
using FacturXManager.Common.Infrastructure.Authentication;
using FacturXManager.Common.Presentation.ApiResults;
using FacturXManager.Common.Presentation.Endpoints;
using FacturXManager.Modules.Users.Application.Users.GetUserPermissions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FacturXManager.Modules.Users.Presentation.Users;

internal sealed class GetUserPermissions : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("users/permissions", async (
            HttpContext context,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            string identityId = context.User.GetIdentityId();

            var query = new GetUserPermissionsQuery(identityId);
            Result<PermissionsResponse> result = await sender.Send(query, cancellationToken);

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization() // ← Juste authentifié (pas besoin de permission spécifique pour voir ses propres permissions)
        .WithTags(Tags.Users)
        .WithName("GetUserPermissions")
        .WithDescription("Get current user permissions");
    }
}
