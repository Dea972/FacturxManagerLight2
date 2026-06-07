using System.Text.Json.Serialization;

namespace FacturXManager.Modules.Directory.Infrastructure.ChorusPro.Dto;

/// <summary>
/// Corps de la requête « Rechercher une structure » par SIRET (POST /structures/v1/rechercher).
/// </summary>
internal sealed record ChorusProSearchStructureRequest
{
    [JsonPropertyName("siret")]
    public string Siret { get; init; } = string.Empty;
}
