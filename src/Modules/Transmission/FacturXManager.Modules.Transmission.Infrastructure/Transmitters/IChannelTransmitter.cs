using FacturXManager.Common.Application.Transmission;

namespace FacturXManager.Modules.Transmission.Infrastructure.Transmitters;

/// <summary>
/// Adaptateur d'acheminement pour un canal donné. Ajouter un canal (PDP, PPF) consiste à
/// enregistrer une nouvelle implémentation, sans toucher à la couche Application (modèle en Y).
/// </summary>
internal interface IChannelTransmitter
{
    TransmissionChannel Channel { get; }

    Task<InvoiceTransmissionResult> TransmitAsync(
        InvoiceTransmissionRequest request,
        CancellationToken cancellationToken = default);
}
