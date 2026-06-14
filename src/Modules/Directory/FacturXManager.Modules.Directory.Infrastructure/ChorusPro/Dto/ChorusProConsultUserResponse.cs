using System.Text.Json.Serialization;

namespace FacturXManager.Modules.Directory.Infrastructure.ChorusPro.Dto;

/// <summary>
/// Réponse de la consultation d'un utilisateur Chorus Pro (POST /utilisateurs/v1/consulter).
/// </summary>
internal sealed record ChorusProConsultUserResponse
{
    [JsonPropertyName("codeRetour")]
    public int ReturnCode { get; init; }

    [JsonPropertyName("libelle")]
    public string? Label { get; init; }

    [JsonPropertyName("login")]
    public string? Login { get; init; }

    [JsonPropertyName("nom")]
    public string? Nom { get; init; }

    [JsonPropertyName("prenom")]
    public string? Prenom { get; init; }

    [JsonPropertyName("email")]
    public string? Email { get; init; }

    [JsonPropertyName("actif")]
    public bool Actif { get; init; }

    [JsonPropertyName("idUtilisateurCourant")]
    public long? IdUtilisateurCourant { get; init; }

    [JsonPropertyName("listeStructures")]
    public List<ChorusProUserStructure> Structures { get; init; } = [];
}

internal sealed record ChorusProUserStructure
{
    [JsonPropertyName("idStructureCPP")]
    public long IdStructureCpp { get; init; }

    [JsonPropertyName("raisonSociale")]
    public string? RaisonSociale { get; init; }

    [JsonPropertyName("siret")]
    public string? Siret { get; init; }
}
