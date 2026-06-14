using FacturXManager.Common.Domain;
using FacturXManager.Modules.Directory.Application.Companies.LookupCompany;
using FacturXManager.Modules.Directory.Application.Structures.ConsultChorusProStructure;

namespace FacturXManager.Modules.Directory.Application.Abstractions;

/// <summary>
/// Port de consultation des structures Chorus Pro via l'API PISTE /structures.
/// </summary>
public interface IChorusProStructureService
{
    /// <summary>Recherche une structure Chorus Pro par SIRET.</summary>
    Task<Result<CompanyLookupResponse>> SearchBySiretAsync(
        string siret,
        CancellationToken cancellationToken = default);

    /// <summary>Retourne le détail d'une structure Chorus Pro par son identifiant CPP.</summary>
    Task<Result<ChorusProStructureDetailResponse>> ConsultByIdAsync(
        long idStructureCpp,
        CancellationToken cancellationToken = default);
}
