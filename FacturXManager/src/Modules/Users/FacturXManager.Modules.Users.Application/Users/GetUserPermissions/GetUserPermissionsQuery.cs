using FacturXManager.Common.Application.Authorization;
using FacturXManager.Common.Application.Messaging;

namespace FacturXManager.Modules.Users.Application.Users.GetUserPermissions;

public sealed record GetUserPermissionsQuery(string IdentityId) : IQuery<PermissionsResponse>;
