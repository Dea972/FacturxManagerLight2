namespace FacturXManager.Modules.Directory.Domain.Companies;

/// <summary>
/// Statut administratif d'une unité légale tel qu'exposé par l'annuaire (INSEE / PPF).
/// </summary>
public enum CompanyLegalStatus
{
    /// <summary>Active : l'entreprise existe et n'est pas cessée.</summary>
    Active = 0,

    /// <summary>Cessée : l'entreprise est radiée ou en cessation d'activité.</summary>
    Ceased = 1,

    /// <summary>Statut inconnu (non renseigné par la source).</summary>
    Unknown = 2,
}
