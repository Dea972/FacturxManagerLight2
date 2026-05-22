using FacturXManager.Common.Domain;
using FacturXManager.Common.Presentation.ApiResults;
using FacturXManager.Common.Presentation.Endpoints;
using FacturXManager.Modules.Users.Application.Users.RegisterUser;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FacturXManager.Modules.Users.Presentation.Users;

internal sealed class RegisterUser : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("users/register", async (RegisterRequest request, ISender sender) =>
        {
            Result<Guid> result = await sender.Send(new RegisterUserCommand(
                request.Email,
                request.UserName,
                request.FirstName,
                request.LastName));

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .AllowAnonymous()
        .WithTags(Tags.Users);
    }

    internal sealed class RegisterRequest
    {
        public string Email { get; init; }

        public string FirstName { get; init; }

        public string LastName { get; init; }

        public string UserName { get; init; }
    }
}
