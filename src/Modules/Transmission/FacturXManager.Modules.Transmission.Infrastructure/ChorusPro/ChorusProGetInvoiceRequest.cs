using System.Text.Json.Serialization;

namespace FacturXManager.Modules.Transmission.Infrastructure.ChorusPro;

/// <summary>
/// Corps de la requête « Consulter facture fournisseur ».
/// </summary>
internal sealed record ChorusProGetInvoiceRequest
{
    [JsonPropertyName("codeLangue")]
    public string LanguageCode { get; init; } = "fr";

    [JsonPropertyName("getpLigneRecapTva")]
    public ChorusProVatSummaryPagination VatSummaryLines { get; init; } = new();

    [JsonPropertyName("getpLignesPoste")]
    public ChorusProInvoiceLinesPagination InvoiceLines { get; init; } = new();

    [JsonPropertyName("getpListePiecesJointes")]
    public ChorusProAttachmentsPagination Attachments { get; init; } = new();

    [JsonPropertyName("idUtilisateurCourant")]
    public int CurrentUserId { get; init; }

    [JsonPropertyName("identifiantFactureCPP")]
    public long InvoiceIdCpp { get; init; }
}

internal sealed record ChorusProVatSummaryPagination
{
    [JsonPropertyName("nbResultatsParPageListeListeRecapitulatifTVA")]
    public int PageSize { get; init; } = 10;

    [JsonPropertyName("pageResultatDemandeeListeRecapitulatifTVA")]
    public int Page { get; init; } = 1;

    [JsonPropertyName("triColonneListeRecapitulatifTVA")]
    public string SortColumn { get; init; } = "LigneTvaMontantTvaParTaux";

    [JsonPropertyName("triSensListeRecapitulatifTVA")]
    public string SortDirection { get; init; } = "Descendant";
}

internal sealed record ChorusProInvoiceLinesPagination
{
    [JsonPropertyName("nbResultatsParPageLignesPoste")]
    public int PageSize { get; init; } = 50;

    [JsonPropertyName("pageResultatDemandeeLignesPoste")]
    public int Page { get; init; } = 1;

    [JsonPropertyName("triColonneListeLignesPoste")]
    public string SortColumn { get; init; } = "LignePosteNumero";

    [JsonPropertyName("triSensListeLignesPoste")]
    public string SortDirection { get; init; } = "Descendant";
}

internal sealed record ChorusProAttachmentsPagination
{
    [JsonPropertyName("nbResultatsParPageListePieceJointe")]
    public int PageSize { get; init; } = 10;

    [JsonPropertyName("pageResultatDemandeeListePieceJointe")]
    public int Page { get; init; } = 1;

    [JsonPropertyName("triColonneListePiecesJointes")]
    public string SortColumn { get; init; } = "PieceJointeTypeCode";

    [JsonPropertyName("triSensListePiecesJointes")]
    public string SortDirection { get; init; } = "Descendant";
}
