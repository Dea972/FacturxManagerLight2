using System.Text.Json.Serialization;

namespace FacturXManager.Modules.Transmission.Infrastructure.ChorusPro;

/// <summary>
/// Réponse Chorus Pro à la consultation de l'historique d'une facture.
/// </summary>
internal sealed record ChorusProHistoryResponse
{
    [JsonPropertyName("codeRetour")]
    public int ReturnCode { get; init; }

    [JsonPropertyName("libelle")]
    public string? Label { get; init; }

    [JsonPropertyName("listeHistoStatuts")]
    public IReadOnlyList<ChorusProHistoryStatus> StatusHistory { get; init; } = [];

    [JsonPropertyName("listeHistoActions")]
    public IReadOnlyList<ChorusProHistoryAction> ActionHistory { get; init; } = [];

    [JsonPropertyName("listeHistoEvenements")]
    public IReadOnlyList<ChorusProHistoryEvent> EventHistory { get; init; } = [];
}

internal sealed record ChorusProHistoryStatus
{
    [JsonPropertyName("codeStatut")]
    public string? StatusCode { get; init; }

    [JsonPropertyName("libelleStatut")]
    public string? StatusLabel { get; init; }

    [JsonPropertyName("dateStatut")]
    public string? StatusDate { get; init; }
}

internal sealed record ChorusProHistoryAction
{
    [JsonPropertyName("codeAction")]
    public string? ActionCode { get; init; }

    [JsonPropertyName("dateAction")]
    public string? ActionDate { get; init; }

    [JsonPropertyName("loginUtilisateur")]
    public string? UserLogin { get; init; }
}

internal sealed record ChorusProHistoryEvent
{
    [JsonPropertyName("libelleEvenement")]
    public string? EventLabel { get; init; }

    [JsonPropertyName("dateEvenement")]
    public string? EventDate { get; init; }
}
