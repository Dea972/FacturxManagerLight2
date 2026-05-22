using FacturXManager.Common.Application.Messaging;

namespace FacturXManager.Modules.Users.Application.Users.UpdateUser;

public sealed record UpdateUserCommand(Guid UserId, string FirstName, string LastName) : ICommand;
