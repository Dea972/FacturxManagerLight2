using System.Security.Claims;
using FacturXManager.Common.Domain;
using FacturXManager.Common.Infrastructure.Authentication;
using FacturXManager.Common.Presentation.ApiResults;
using FacturXManager.Common.Presentation.Endpoints;
using FacturXManager.Modules.Users.Application.Users;
using FacturXManager.Modules.Users.Application.Users.GetUser;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FacturXManager.Modules.Users.Presentation.Users;

internal sealed class GetUserProfile : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("users/profile", async (ClaimsPrincipal claims, ISender sender) =>
        {
            Result<UserResponse> result = await sender.Send(new GetUserQuery(claims.GetUserId()));

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization(Permissions.GetUser)
        .WithTags(Tags.Users);
    }
}
