namespace FacturXManager.Client.Workflows;

/// <summary>
/// Point d'entrée unique du SDK FacturX Manager côté client.
/// Regroupe les clients spécialisés par domaine fonctionnel.
/// </summary>
public sealed class FacturXManagerClient(InvoiceWorkflowClient workflows)
{
    /// <summary>
    /// Flux métier orchestrés : génération + dépôt en un seul appel.
    /// </summary>
    public InvoiceWorkflowClient Workflows { get; } = workflows;
}
