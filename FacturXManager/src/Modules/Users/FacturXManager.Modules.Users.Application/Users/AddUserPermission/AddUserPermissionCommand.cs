using FacturXManager.Common.Application.Messaging;

namespace FacturXManager.Modules.Users.Application.Users.AddUserPermission;

public sealed record AddUserPermissionCommand(Guid UserId, string PermissionCode) : ICommand;
