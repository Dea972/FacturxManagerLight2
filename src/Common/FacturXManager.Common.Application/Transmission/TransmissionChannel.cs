namespace FacturXManager.Common.Application.Transmission;

/// <summary>
/// Canaux d'acheminement de la réforme (modèle en Y). Permet d'ajouter PDP/PPF
/// sans modifier la couche Application des modules appelants.
/// </summary>
public enum TransmissionChannel
{
    /// <summary>Plateforme publique de facturation / Chorus Pro (PPF historique).</summary>
    ChorusPro = 0,

    /// <summary>Plateforme de dématérialisation partenaire.</summary>
    Pdp = 1,

    /// <summary>Portail public de facturation.</summary>
    Ppf = 2
}
