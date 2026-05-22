using FacturXManager.Common.Domain;
using FacturXManager.Common.Presentation.ApiResults;
using FacturXManager.Common.Presentation.Endpoints;
using FacturXManager.Modules.Users.Application.Users.RemoveUserPermission;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FacturXManager.Modules.Users.Presentation.Users;

internal sealed class RemoveUserPermission : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("users/{id:guid}/permissions/{permissionCode}", async (Guid id, string permissionCode, ISender sender) =>
        {
            var command = new RemoveUserPermissionCommand(id, permissionCode);

            Result result = await sender.Send(command);

            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        })
        .RequireAuthorization(Permissions.AdminAccess)
        .WithTags(Tags.Users);
    }
}
