namespace FacturXManager.Modules.Directory.Infrastructure.Sirene;

/// <summary>
/// Options de connexion à l'API SIRENE (INSEE). Le token Bearer est renseigné via configuration
/// ou variables d'environnement, jamais en dur, et n'est jamais loggué.
/// </summary>
public sealed class SireneOptions
{
    public const string SectionName = "Directory:Sirene";

    /// <summary>URL de base de l'API SIRENE (ex : <c>https://api.insee.fr/api-sirene/3.11/</c>).</summary>
    public string BaseUrl { get; init; } = string.Empty;

    /// <summary>Token Bearer permanent issu du portail INSEE. Confidentiel.</summary>
    public string ApiKey { get; init; } = string.Empty;

    /// <summary>Durée de mise en cache des résultats de consultation (par défaut 15 minutes).</summary>
    public int CacheDurationMinutes { get; init; } = 15;
}
