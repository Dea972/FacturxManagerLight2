namespace FacturXManager.Modules.Transmission.Domain.Transmissions;

/// <summary>
/// Cycle de vie d'un dépôt. Le rejet métier (Rejected) est distinct de l'échec technique.
/// </summary>
public enum TransmissionStatus
{
    /// <summary>En attente de dépôt.</summary>
    Pending = 0,

    /// <summary>Déposé et accepté par la plateforme, en attente de traitement.</summary>
    Submitted = 1,

    /// <summary>Accepté par la plateforme destinataire.</summary>
    Accepted = 2,

    /// <summary>Rejeté par la plateforme (rejet métier).</summary>
    Rejected = 3,

    /// <summary>Échec technique (réseau, indisponibilité plateforme).</summary>
    TechnicalFailure = 4
}
