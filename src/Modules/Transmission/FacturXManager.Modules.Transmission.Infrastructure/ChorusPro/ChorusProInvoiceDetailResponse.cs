using System.Text.Json.Serialization;

namespace FacturXManager.Modules.Transmission.Infrastructure.ChorusPro;

/// <summary>
/// Réponse Chorus Pro au détail d'une facture fournisseur.
/// </summary>
internal sealed record ChorusProInvoiceDetailResponse
{
    [JsonPropertyName("codeRetour")]
    public int ReturnCode { get; init; }

    [JsonPropertyName("libelle")]
    public string? Label { get; init; }

    [JsonPropertyName("identifiantFactureCPP")]
    public long InvoiceIdCpp { get; init; }

    [JsonPropertyName("numeroFacture")]
    public string? InvoiceNumber { get; init; }

    [JsonPropertyName("statutFacture")]
    public string? InvoiceStatus { get; init; }

    [JsonPropertyName("montantTtcTotal")]
    public decimal TotalAmountInclTax { get; init; }

    [JsonPropertyName("listeRecapitulatifTVA")]
    public IReadOnlyList<ChorusProVatSummaryLine> VatSummaryLines { get; init; } = [];

    [JsonPropertyName("listeLignesPoste")]
    public IReadOnlyList<ChorusProInvoiceLine> InvoiceLines { get; init; } = [];

    [JsonPropertyName("listePiecesJointes")]
    public IReadOnlyList<ChorusProAttachment> Attachments { get; init; } = [];
}

internal sealed record ChorusProVatSummaryLine
{
    [JsonPropertyName("tauxTva")]
    public decimal VatRate { get; init; }

    [JsonPropertyName("montantTva")]
    public decimal VatAmount { get; init; }

    [JsonPropertyName("montantBaseHtTva")]
    public decimal TaxableBaseAmount { get; init; }
}

internal sealed record ChorusProInvoiceLine
{
    [JsonPropertyName("numeroLignePoste")]
    public int LineNumber { get; init; }

    [JsonPropertyName("libelleLignePoste")]
    public string? Description { get; init; }

    [JsonPropertyName("quantite")]
    public decimal Quantity { get; init; }

    [JsonPropertyName("prixUnitaire")]
    public decimal UnitPrice { get; init; }

    [JsonPropertyName("montantTtc")]
    public decimal AmountInclTax { get; init; }
}

internal sealed record ChorusProAttachment
{
    [JsonPropertyName("typeCodePieceJointe")]
    public string? AttachmentTypeCode { get; init; }

    [JsonPropertyName("nomFichier")]
    public string? FileName { get; init; }
}
