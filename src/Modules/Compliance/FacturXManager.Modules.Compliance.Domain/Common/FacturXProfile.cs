namespace FacturXManager.Modules.Compliance.Domain.Common;

/// <summary>
/// Profils de conformité Factur-X (du moins riche au plus riche).
/// EN16931 (Comfort) est le profil cible prioritaire de la réforme française.
/// </summary>
public enum FacturXProfile
{
    Minimum = 0,
    BasicWl = 1,
    Basic = 2,
    En16931 = 3,
    Extended = 4
}
