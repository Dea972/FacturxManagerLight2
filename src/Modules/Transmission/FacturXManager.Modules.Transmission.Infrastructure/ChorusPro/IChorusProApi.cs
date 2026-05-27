using Refit;

namespace FacturXManager.Modules.Transmission.Infrastructure.ChorusPro;

/// <summary>
/// Client Refit de l'API Factures Chorus Pro (PISTE).
/// </summary>
internal interface IChorusProApi
{
    [Post("/cpro/factures/v1/deposer/flux")]
    Task<ApiResponse<ChorusProDepositResponse>> DepositFluxAsync(
        [Body] ChorusProDepositRequest request,
        CancellationToken cancellationToken = default);

    [Post("/cpro/factures/v1/rechercher/fournisseur")]
    Task<ApiResponse<ChorusProSearchResponse>> SearchSupplierInvoicesAsync(
        [Body] ChorusProSearchInvoicesRequest request,
        CancellationToken cancellationToken = default);

    [Post("/cpro/factures/v1/consulter/fournisseur")]
    Task<ApiResponse<ChorusProInvoiceDetailResponse>> GetSupplierInvoiceAsync(
        [Body] ChorusProGetInvoiceRequest request,
        CancellationToken cancellationToken = default);

    [Post("/cpro/factures/v1/consulter/historique")]
    Task<ApiResponse<ChorusProHistoryResponse>> GetInvoiceHistoryAsync(
        [Body] ChorusProGetHistoryRequest request,
        CancellationToken cancellationToken = default);

    [Post("/cpro/factures/v1/recycler")]
    Task<ApiResponse<ChorusProRecycleResponse>> RecycleInvoiceAsync(
        [Body] ChorusProRecycleInvoiceRequest request,
        CancellationToken cancellationToken = default);
}
