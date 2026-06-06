using System.Text.Json.Serialization;

namespace FacturXManager.Modules.Transmission.Infrastructure.ChorusPro;

/// <summary>
/// Réponse Chorus Pro au recyclage d'une facture.
/// </summary>
internal sealed record ChorusProRecycleResponse
{
    [JsonPropertyName("codeRetour")]
    public int ReturnCode { get; init; }

    [JsonPropertyName("libelle")]
    public string? Label { get; init; }
}
