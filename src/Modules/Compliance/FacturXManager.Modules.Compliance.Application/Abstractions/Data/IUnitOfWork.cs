namespace FacturXManager.Modules.Compliance.Application.Abstractions.Data;

/// <summary>
/// Unité de travail du module Compliance : valide les changements en base.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
