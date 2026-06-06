using System.Text.Json.Serialization;

namespace FacturXManager.Modules.Transmission.Infrastructure.ChorusPro;

/// <summary>
/// Corps de la requête « Recycler une facture ».
/// </summary>
internal sealed record ChorusProRecycleInvoiceRequest
{
    [JsonPropertyName("commentaire")]
    public string? Comment { get; init; }

    [JsonPropertyName("destinataire")]
    public ChorusProRecycleRecipient? Recipient { get; init; }

    [JsonPropertyName("idUtilisateurCourant")]
    public int CurrentUserId { get; init; }

    [JsonPropertyName("identifiantFactureCPP")]
    public long InvoiceIdCpp { get; init; }

    [JsonPropertyName("reference")]
    public ChorusProRecycleReference? Reference { get; init; }
}

internal sealed record ChorusProRecycleRecipient
{
    [JsonPropertyName("codeDestinataire")]
    public string RecipientCode { get; init; } = string.Empty;

    [JsonPropertyName("codeServiceExecutant")]
    public string? ExecutingServiceCode { get; init; }
}

internal sealed record ChorusProRecycleReference
{
    [JsonPropertyName("deviseFacture")]
    public string? CurrencyCode { get; init; }

    [JsonPropertyName("modePaiement")]
    public string? PaymentMode { get; init; }

    [JsonPropertyName("motifExonerationTva")]
    public string? VatExemptionReason { get; init; }

    [JsonPropertyName("numeroBonCommande")]
    public string? PurchaseOrderNumber { get; init; }

    [JsonPropertyName("numeroFactureOrigine")]
    public string? OriginalInvoiceNumber { get; init; }

    [JsonPropertyName("numeroMarche")]
    public string? ContractNumber { get; init; }

    [JsonPropertyName("typeFacture")]
    public string? InvoiceType { get; init; }

    [JsonPropertyName("typeTva")]
    public string? VatType { get; init; }
}
