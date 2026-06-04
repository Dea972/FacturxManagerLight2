namespace FacturXManager.Modules.Compliance.Domain.Common;

/// <summary>
/// Constantes réglementaires Factur-X.
/// </summary>
public static class FacturXConstants
{
    /// <summary>Nom imposé du fichier XML embarqué dans le PDF/A-3b.</summary>
    public const string EmbeddedXmlFileName = "factur-x.xml";

    /// <summary>Identifiant de spécification (Guideline ID) du profil EN16931 (Comfort).</summary>
    public const string En16931GuidelineId = "urn:cen.eu:en16931:2017";

    /// <summary>Identifiant de spécification du profil Extended (extension française Factur-X).</summary>
    public const string ExtendedGuidelineId =
        "urn:cen.eu:en16931:2017#conformant#urn:factur-x.eu:1p0:extended";

    /// <summary>
    /// Date d'entrée en vigueur de l'obligation B2B (SIREN acheteur + nature d'opération).
    /// </summary>
    public static readonly DateOnly B2BObligationStartDate = new(2026, 9, 1);
}
