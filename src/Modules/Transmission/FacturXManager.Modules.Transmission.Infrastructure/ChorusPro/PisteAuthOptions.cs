namespace FacturXManager.Modules.Transmission.Infrastructure.ChorusPro;

/// <summary>
/// Options OAuth2 PISTE (AIFE). Renseignées via configuration / variables d'environnement,
/// jamais en dur. Le secret n'est jamais loggué.
/// </summary>
public sealed class PisteAuthOptions
{
    public const string SectionName = "Transmission:Piste";

    public string TokenEndpoint { get; init; } = string.Empty;

    public string ChorusProBaseUrl { get; init; } = string.Empty;

    public string ClientId { get; init; } = string.Empty;

    public string ClientSecret { get; init; } = string.Empty;

    public string Scope { get; init; } = "openid";

    /// <summary>Marge de renouvellement du token avant expiration (par défaut 60 s).</summary>
    public int RenewBeforeExpirySeconds { get; init; } = 60;
}
