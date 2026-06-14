namespace FacturXManager.Modules.Directory.Infrastructure.ChorusPro;

/// <summary>
/// Options spécifiques au module Directory pour l'interrogation de l'annuaire Chorus Pro.
/// Réutilise les credentials PISTE définis dans "Transmission:Piste".
/// </summary>
internal sealed class ChorusProOptions
{
    public const string SectionName = "Transmission:Piste";

    /// <summary>URL de base des APIs structures/utilisateurs PISTE Chorus Pro.</summary>
    public string ChorusProBaseUrl { get; init; } = string.Empty;

    /// <summary>Durée de mise en cache des résultats de consultation (par défaut 60 minutes).</summary>
    public int CacheDurationMinutes { get; init; } = 60;
}
