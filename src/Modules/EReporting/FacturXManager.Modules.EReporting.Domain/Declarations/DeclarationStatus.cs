namespace FacturXManager.Modules.EReporting.Domain.Declarations;

/// <summary>
/// Cycle de vie d'une déclaration d'e-reporting DGFiP.
/// </summary>
public enum DeclarationStatus
{
    /// <summary>Brouillon : modifiable, accumule des transactions.</summary>
    Draft = 0,

    /// <summary>Finalisée : figée, prête à être soumise.</summary>
    Finalized = 1,

    /// <summary>Soumise à la DGFiP, en attente de traitement.</summary>
    Submitted = 2,

    /// <summary>Acceptée par la DGFiP.</summary>
    Accepted = 3
}
