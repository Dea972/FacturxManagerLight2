namespace FacturXManager.Common.Application.Transmission;

/// <summary>
/// Issue d'une tentative de transmission. Distingue le rejet métier de l'échec technique.
/// </summary>
public enum TransmissionOutcome
{
    /// <summary>Le document a été déposé et accepté par la plateforme (en attente de traitement).</summary>
    Submitted = 0,

    /// <summary>Le document a été rejeté par la plateforme (rejet métier).</summary>
    Rejected = 1,

    /// <summary>Une erreur technique a empêché la transmission (réseau, indisponibilité, etc.).</summary>
    TechnicalFailure = 2
}
