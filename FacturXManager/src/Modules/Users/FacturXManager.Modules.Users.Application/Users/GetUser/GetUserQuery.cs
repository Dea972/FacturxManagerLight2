using System;
using System.Collections.Generic;
using System.Text;
using FacturXManager.Common.Application.Messaging;

namespace FacturXManager.Modules.Users.Application.Users.GetUser;

public sealed record class GetUserQuery(Guid UserId) : IQuery<UserResponse>;
