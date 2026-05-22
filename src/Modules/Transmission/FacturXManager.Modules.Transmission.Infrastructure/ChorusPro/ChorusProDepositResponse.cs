using System.Text.Json.Serialization;

namespace FacturXManager.Modules.Transmission.Infrastructure.ChorusPro;

/// <summary>
/// Réponse Chorus Pro au dépôt d'un flux. <c>codeRetour == 0</c> indique un dépôt accepté.
/// </summary>
internal sealed record ChorusProDepositResponse
{
    [JsonPropertyName("codeRetour")]
    public int ReturnCode { get; init; }

    [JsonPropertyName("libelle")]
    public string? Label { get; init; }

    [JsonPropertyName("dateDepot")]
    public string? DepositDate { get; init; }

    [JsonPropertyName("numeroFluxDepot")]
    public string? DepositFluxNumber { get; init; }
}
