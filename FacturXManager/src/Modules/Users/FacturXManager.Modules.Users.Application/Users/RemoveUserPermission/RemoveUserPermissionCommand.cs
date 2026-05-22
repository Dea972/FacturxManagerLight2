using FacturXManager.Common.Application.Messaging;

namespace FacturXManager.Modules.Users.Application.Users.RemoveUserPermission;

public sealed record RemoveUserPermissionCommand(Guid UserId, string PermissionCode) : ICommand;
