using System.Text.Json.Serialization;

namespace FacturXManager.Modules.Transmission.Infrastructure.ChorusPro;

/// <summary>
/// Corps de la requête « Déposer un flux » Chorus Pro en mode DEPOT_PDF_API.
/// </summary>
internal sealed record ChorusProDepositRequest
{
    [JsonPropertyName("avecSignature")]
    public bool WithSignature { get; init; }

    [JsonPropertyName("fichierFlux")]
    public string FileContentBase64 { get; init; } = string.Empty;

    [JsonPropertyName("nomFichier")]
    public string FileName { get; init; } = string.Empty;

    [JsonPropertyName("syntaxeFlux")]
    public string FluxSyntax { get; init; } = "IN_DP_E2_FACTURX";
}
