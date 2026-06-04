using FacturXManager.Common.Domain;

namespace FacturXManager.Modules.Transmission.Domain.Transmissions;

/// <summary>
/// Trace d'une tentative de dépôt : horodatage, statut résultant et réponse brute de la plateforme.
/// </summary>
public sealed class TransmissionAttempt : Entity
{
    private TransmissionAttempt()
    {
    }

    public Guid Id { get; private set; }

    public Guid TransmissionId { get; private set; }

    public TransmissionStatus Status { get; private set; }

    public DateTime OccurredOnUtc { get; private set; }

    public string? ChannelDepositId { get; private set; }

    /// <summary>Réponse brute de la plateforme (jamais le contenu Base64 du PDF).</summary>
    public string? RawResponse { get; private set; }

    public string? Message { get; private set; }

    internal static TransmissionAttempt Create(
        Guid transmissionId,
        TransmissionStatus status,
        DateTime occurredOnUtc,
        string? channelDepositId,
        string? rawResponse,
        string? message) =>
        new()
        {
            Id = Guid.CreateVersion7(),
            TransmissionId = transmissionId,
            Status = status,
            OccurredOnUtc = occurredOnUtc,
            ChannelDepositId = channelDepositId,
            RawResponse = rawResponse,
            Message = message,
        };
}
