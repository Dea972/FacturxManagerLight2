using System.Text.Json.Serialization;

namespace FacturXManager.Modules.Directory.Infrastructure.ChorusPro.Dto;

/// <summary>
/// Réponse de la consultation détaillée d'une structure Chorus Pro (POST /structures/v1/consulter).
/// </summary>
internal sealed record ChorusProConsultStructureResponse
{
    [JsonPropertyName("codeRetour")]
    public int ReturnCode { get; init; }

    [JsonPropertyName("libelle")]
    public string? Label { get; init; }

    [JsonPropertyName("idStructureCPP")]
    public long IdStructureCpp { get; init; }

    [JsonPropertyName("siret")]
    public string? Siret { get; init; }

    [JsonPropertyName("raisonSociale")]
    public string? RaisonSociale { get; init; }

    [JsonPropertyName("codeStatut")]
    public string? CodeStatut { get; init; }

    [JsonPropertyName("libStatut")]
    public string? LibStatut { get; init; }

    [JsonPropertyName("identifiantTechnique")]
    public string? IdentifiantTechnique { get; init; }

    [JsonPropertyName("typeIdentifiantStructure")]
    public string? TypeIdentifiantStructure { get; init; }

    [JsonPropertyName("codeServiceDlin")]
    public string? CodeServiceDlin { get; init; }

    [JsonPropertyName("libServiceDlin")]
    public string? LibServiceDlin { get; init; }

    [JsonPropertyName("nombreServices")]
    public int NombreServices { get; init; }
}
