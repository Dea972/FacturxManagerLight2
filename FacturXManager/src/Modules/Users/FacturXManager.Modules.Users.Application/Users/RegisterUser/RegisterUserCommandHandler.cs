using System;
using System.Collections.Generic;
using System.Text;
using FacturXManager.Common.Application.Messaging;
using FacturXManager.Common.Domain;
using FacturXManager.Modules.Users.Application.Abstractions.Data;
using FacturXManager.Modules.Users.Application.Abstractions.Identity;
using FacturXManager.Modules.Users.Domain.Users;

namespace FacturXManager.Modules.Users.Application.Users.RegisterUser;

internal sealed class RegisterUserCommandHandler(
    IIdentityProviderService identityProviderService)
    : ICommandHandler<RegisterUserCommand, Guid>
{
    public async Task<Result<Guid>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        Result<Guid> result = await identityProviderService.RegisterUserAsync(
            new UserModel(request.UserName,request.Email,  request.FirstName, request.LastName),
            cancellationToken);

        if (result.IsFailure)
        {
            return Result.Failure<Guid>(result.Error);
        }

        return result.Value;
    }
}
