namespace FacturXManager.Common.Application.Transmission;

/// <summary>
/// Port d'acheminement d'un document certifié vers une plateforme (Chorus Pro, PDP, PPF).
/// Abstraction du Shared Kernel : la couche Application passe toujours par ce port,
/// jamais par un adaptateur concret. L'implémentation route selon le canal demandé.
/// </summary>
public interface IInvoiceTransmitter
{
    Task<InvoiceTransmissionResult> TransmitAsync(
        InvoiceTransmissionRequest request,
        CancellationToken cancellationToken = default);
}
