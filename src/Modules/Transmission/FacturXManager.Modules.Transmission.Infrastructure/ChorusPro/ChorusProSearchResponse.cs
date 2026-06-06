using System.Text.Json.Serialization;

namespace FacturXManager.Modules.Transmission.Infrastructure.ChorusPro;

/// <summary>
/// Réponse Chorus Pro à la recherche de factures fournisseur.
/// </summary>
internal sealed record ChorusProSearchResponse
{
    [JsonPropertyName("codeRetour")]
    public int ReturnCode { get; init; }

    [JsonPropertyName("libelle")]
    public string? Label { get; init; }

    [JsonPropertyName("listeFactures")]
    public IReadOnlyList<ChorusProInvoiceSummary> Invoices { get; init; } = [];
}

internal sealed record ChorusProInvoiceSummary
{
    [JsonPropertyName("identifiantFactureCPP")]
    public long InvoiceIdCpp { get; init; }

    [JsonPropertyName("numeroFacture")]
    public string? InvoiceNumber { get; init; }

    [JsonPropertyName("statutFacture")]
    public string? InvoiceStatus { get; init; }

    [JsonPropertyName("montantTtcTotal")]
    public decimal TotalAmountInclTax { get; init; }
}
