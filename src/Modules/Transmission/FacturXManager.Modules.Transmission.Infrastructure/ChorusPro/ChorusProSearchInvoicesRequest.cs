using System.Text.Json.Serialization;

namespace FacturXManager.Modules.Transmission.Infrastructure.ChorusPro;

/// <summary>
/// Corps de la requête « Rechercher factures fournisseur ».
/// </summary>
internal sealed record ChorusProSearchInvoicesRequest
{
    [JsonPropertyName("typeFacture")]
    public string InvoiceType { get; init; } = "FACTURE";

    [JsonPropertyName("numeroFluxDepot")]
    public string? DepositFluxNumber { get; init; }

    [JsonPropertyName("rechercheFactureParFournisseur")]
    public ChorusProSearchPaginationRequest Pagination { get; init; } = new();
}

internal sealed record ChorusProSearchPaginationRequest
{
    [JsonPropertyName("nbResultatsParPage")]
    public int PageSize { get; init; } = 50;

    [JsonPropertyName("pageResultatDemandee")]
    public int Page { get; init; } = 1;

    [JsonPropertyName("triColonne")]
    public string SortColumn { get; init; } = "TypeDemandePaiement";

    [JsonPropertyName("triSens")]
    public string SortDirection { get; init; } = "Descendant";
}
