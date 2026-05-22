using FacturXManager.Common.Application.Messaging;

namespace FacturXManager.Modules.Users.Application.Users.ChangeUserRole;

public sealed record ChangeUserRoleCommand(Guid UserId, string RoleName) : ICommand;
