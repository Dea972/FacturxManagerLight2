using System.Text.Json.Serialization;

namespace FacturXManager.Common.Infrastructure.Piste;

/// <summary>
/// Réponse du serveur OAuth2 PISTE (flux client_credentials).
/// </summary>
internal sealed record PisteTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; init; } = string.Empty;

    [JsonPropertyName("token_type")]
    public string TokenType { get; init; } = "Bearer";

    [JsonPropertyName("expires_in")]
    public int ExpiresInSeconds { get; init; }
}
