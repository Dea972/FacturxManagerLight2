using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FacturXManager.Modules.Directory.Infrastructure.ChorusPro.Dto;

/// <summary>
/// Réponse de la recherche de structures Chorus Pro (POST /structures/v1/rechercher).
/// </summary>
internal sealed record ChorusProSearchStructureResponse
{
    [JsonPropertyName("codeRetour")]
    public int ReturnCode { get; init; }

    [JsonPropertyName("libelle")]
    public string? Label { get; init; }

    [JsonPropertyName("nbResultats")]
    public int ResultCount { get; init; }

    [JsonPropertyName("listeStructures")]
    public List<ChorusProStructureSummary> Structures { get; init; } = [];
}

internal sealed record ChorusProStructureSummary
{
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
}
