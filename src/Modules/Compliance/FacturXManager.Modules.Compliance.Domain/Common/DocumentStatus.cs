namespace FacturXManager.Modules.Compliance.Domain.Common;

/// <summary>
/// Cycle de vie d'un document Factur-X (facture ou avoir) au sein du module Compliance.
/// </summary>
public enum DocumentStatus
{
    /// <summary>Créé en mémoire, non encore certifié.</summary>
    Draft = 0,

    /// <summary>Certifié : XML CII + PDF/A-3b générés et persistés.</summary>
    Certified = 1,

    /// <summary>Déposé avec succès sur une plateforme (ex : Chorus Pro).</summary>
    Submitted = 2,

    /// <summary>Échec du dépôt (rejet plateforme ou échec technique).</summary>
    SubmissionFailed = 3
}
