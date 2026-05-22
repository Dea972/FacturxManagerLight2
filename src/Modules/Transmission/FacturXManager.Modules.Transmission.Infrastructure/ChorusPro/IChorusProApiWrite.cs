using Refit;

namespace FacturXManager.Modules.Transmission.Infrastructure.ChorusPro;

/// <summary>
/// Client Refit (écriture) de l'API Factures Chorus Pro.
/// </summary>
internal interface IChorusProApiWrite
{
    [Post("/cpro/factures/v1/deposer/flux")]
    Task<ApiResponse<ChorusProDepositResponse>> DepositFluxAsync(
        [Body] ChorusProDepositRequest request,
        CancellationToken cancellationToken = default);
}
