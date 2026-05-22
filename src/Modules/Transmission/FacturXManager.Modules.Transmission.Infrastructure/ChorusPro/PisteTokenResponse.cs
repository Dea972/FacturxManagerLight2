using System.Text.Json.Serialization;

namespace FacturXManager.Modules.Transmission.Infrastructure.ChorusPro;

internal sealed record PisteTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; init; } = string.Empty;

    [JsonPropertyName("token_type")]
    public string TokenType { get; init; } = "Bearer";

    [JsonPropertyName("expires_in")]
    public int ExpiresInSeconds { get; init; }
}
