
using FacturXManager.Common.Domain;

namespace FacturXManager.Common.Application.Authorization;

public interface IPermissionService
{
    Task<Result<PermissionsResponse>> GetUserPermissionsAsync(string identityId);
}
