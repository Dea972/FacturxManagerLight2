using FacturXManager.Common.Application.Authorization;
using FacturXManager.Common.Domain;
using FacturXManager.Modules.Users.Application.Users.GetUserPermissions;
using MediatR;

namespace FacturXManager.Modules.Users.Infrastructure.Authorization;

internal sealed class PermissionService(ISender sender) : IPermissionService
{
    public async Task<Result<PermissionsResponse>> GetUserPermissionsAsync(string identityId)
    {
        return await sender.Send(new GetUserPermissionsQuery(identityId));
    }
}
