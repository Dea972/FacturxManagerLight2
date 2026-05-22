using FacturXManager.Common.Application.Messaging;

namespace FacturXManager.Modules.Users.Application.Users.GetAllUsers;

public sealed record GetAllUsersQuery : IQuery<IEnumerable<UserDetailResponse>>;
