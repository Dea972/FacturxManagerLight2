using System;
using System.Collections.Generic;
using System.Text;

namespace FacturXManager.Modules.Users.Application.Abstractions.Data;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
