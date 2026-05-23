using FacturXManager.Common.Application.Transmission;
using Microsoft.Extensions.Logging;

namespace FacturXManager.Modules.Transmission.Infrastructure.Transmitters;

/// <summary>
/// Implémentation du port <see cref="IInvoiceTransmitter"/> qui route vers l'adaptateur du canal demandé.
/// </summary>
internal sealed class CompositeInvoiceTransmitter(
    IEnumerable<IChannelTransmitter> channelTransmitters,
    ILogger<CompositeInvoiceTransmitter> logger)
    : IInvoiceTransmitter
{
    public Task<InvoiceTransmissionResult> TransmitAsync(
        InvoiceTransmissionRequest request,
        CancellationToken cancellationToken = default)
    {
        IChannelTransmitter? transmitter =
            channelTransmitters.FirstOrDefault(t => t.Channel == request.Channel);

        if (transmitter is null)
        {
            logger.LogWarning("Aucun adaptateur d'acheminement n'est configuré pour le canal {Channel}", request.Channel);

            return Task.FromResult(
                InvoiceTransmissionResult.TechnicalFailure($"Canal non supporté : {request.Channel}"));
        }

        return transmitter.TransmitAsync(request, cancellationToken);
    }
}
