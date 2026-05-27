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

    /// <summary>Valeur encodée base64(login:password) du compte technique Chorus Pro.</summary>
    public string CproAccountBase64 => Convert.ToBase64String(
        System.Text.Encoding.UTF8.GetBytes($"{CproLogin}:{CproPassword}"));

    public string CproLogin { get; init; } = string.Empty;

    public string CproPassword { get; init;} = string.Empty;
}
