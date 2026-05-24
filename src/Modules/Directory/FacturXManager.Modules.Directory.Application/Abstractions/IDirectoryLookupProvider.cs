using FacturXManager.Common.Domain;
using FacturXManager.Modules.Directory.Domain.Companies;

namespace FacturXManager.Modules.Directory.Application.Abstractions;

/// <summary>
/// Port de consultation annuaire des entreprises. SIRENE aujourd'hui, annuaire PPF demain.
/// L'implémentation concrète (HTTP, cache, fallback) est portée par l'Infrastructure.
/// </summary>
public interface IDirectoryLookupProvider
{
    Task<Result<CompanyDirectoryEntry>> LookupBySirenAsync(Siren siren, CancellationToken cancellationToken = default);

    Task<Result<CompanyDirectoryEntry>> LookupBySiretAsync(Siret siret, CancellationToken cancellationToken = default);
}
