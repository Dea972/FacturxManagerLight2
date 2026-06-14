using System.Text.Json.Serialization;

namespace FacturXManager.Modules.Directory.Infrastructure.ChorusPro.Dto;

/// <summary>
/// Corps de la requête « Consulter le détail d'une structure » (POST /structures/v1/consulter).
/// </summary>
internal sealed record ChorusProConsultStructureRequest
{
    [JsonPropertyName("idStructureCPP")]
    public long IdStructureCpp { get; init; }
}
