using System;
using System.Collections.Generic;
using System.Text;

namespace FacturXManager.Modules.Users.Domain.Users;

public interface IUserRepository
{
    Task<User?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<User?> GetAsync(string identityId, CancellationToken cancellationToken = default);

    void Insert(User user);

    void UntrackStaticEntities();
}

