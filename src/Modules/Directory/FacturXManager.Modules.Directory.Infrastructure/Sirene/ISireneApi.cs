using FacturXManager.Modules.Directory.Infrastructure.Sirene.Dto;
using Refit;

namespace FacturXManager.Modules.Directory.Infrastructure.Sirene;

/// <summary>
/// Client Refit (lecture seule) de l'API SIRENE (INSEE).
/// </summary>
internal interface ISireneApi
{
    [Get("/siren/{siren}")]
    Task<ApiResponse<SireneUniteLegaleResponse>> GetUniteLegaleAsync(
        string siren,
        CancellationToken cancellationToken = default);

    [Get("/siret/{siret}")]
    Task<ApiResponse<SireneEtablissementDto>> GetEtablissementAsync(
        string siret,
        CancellationToken cancellationToken = default);
}
