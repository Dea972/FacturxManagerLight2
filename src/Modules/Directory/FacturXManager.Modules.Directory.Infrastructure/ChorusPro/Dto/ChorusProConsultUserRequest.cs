using System.Text.Json.Serialization;

namespace FacturXManager.Modules.Directory.Infrastructure.ChorusPro.Dto;

/// <summary>
/// Corps de la requête « Consulter un utilisateur Chorus Pro » (POST /utilisateurs/v1/consulter).
/// </summary>
internal sealed record ChorusProConsultUserRequest
{
    [JsonPropertyName("login")]
    public string Login { get; init; } = string.Empty;
}
