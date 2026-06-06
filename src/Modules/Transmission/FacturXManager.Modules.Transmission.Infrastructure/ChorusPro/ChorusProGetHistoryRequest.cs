using System.Text.Json.Serialization;

namespace FacturXManager.Modules.Transmission.Infrastructure.ChorusPro;

/// <summary>
/// Corps de la requête « Consulter historique d'une facture ».
/// </summary>
internal sealed record ChorusProGetHistoryRequest
{
    [JsonPropertyName("idFacture")]
    public long InvoiceId { get; init; }

    [JsonPropertyName("idUtilisateurCourant")]
    public int CurrentUserId { get; init; }

    [JsonPropertyName("nbResultatsMaximum")]
    public int MaxResults { get; init; } = 50;

    [JsonPropertyName("paramRechercheHistoActionsUtilisateurs")]
    public ChorusProHistoActionsPagination ActionsPagination { get; init; } = new();

    [JsonPropertyName("paramRechercheHistoEvenementsComplementaires")]
    public ChorusProHistoEventsPagination EventsPagination { get; init; } = new();

    [JsonPropertyName("paramRechercheHistoStatuts")]
    public ChorusProHistoStatusPagination StatusPagination { get; init; } = new();
}

internal sealed record ChorusProHistoActionsPagination
{
    [JsonPropertyName("nbResultatsParPageListeHistoAction")]
    public int PageSize { get; init; } = 10;

    [JsonPropertyName("pageResultatDemandeeListeHistoAction")]
    public int Page { get; init; } = 1;

    [JsonPropertyName("triColonneListeHistoAction")]
    public string SortColumn { get; init; } = "HistoActionDateHeure";

    [JsonPropertyName("triSensListeHistoAction")]
    public string SortDirection { get; init; } = "Descendant";
}

internal sealed record ChorusProHistoEventsPagination
{
    [JsonPropertyName("nbResultatsParPageListeHistoEvenement")]
    public int PageSize { get; init; } = 10;

    [JsonPropertyName("pageResultatDemandeeListeHistoEvenement")]
    public int Page { get; init; } = 1;

    [JsonPropertyName("triColonneListeHistoEvenement")]
    public string SortColumn { get; init; } = "EvenementLibelle";

    [JsonPropertyName("triSensListeHistoEvenement")]
    public string SortDirection { get; init; } = "Descendant";
}

internal sealed record ChorusProHistoStatusPagination
{
    [JsonPropertyName("nbResultatsParPageListeHistoStatut")]
    public int PageSize { get; init; } = 10;

    [JsonPropertyName("pageResultatDemandeeListeHistoStatut")]
    public int Page { get; init; } = 1;

    [JsonPropertyName("triColonneListeHistoStatut")]
    public string SortColumn { get; init; } = "HistoStatutCode";

    [JsonPropertyName("triSensListeHistoStatut")]
    public string SortDirection { get; init; } = "Descendant";
}
