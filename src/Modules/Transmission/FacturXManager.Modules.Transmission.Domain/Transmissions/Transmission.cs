using FacturXManager.Common.Domain;

namespace FacturXManager.Modules.Transmission.Domain.Transmissions;

/// <summary>
/// Agrégat de transmission : achemine un document certifié (référencé, jamais son contenu)
/// vers une plateforme et trace chaque tentative de dépôt.
/// </summary>
public sealed class Transmission : Entity
{
    private readonly List<TransmissionAttempt> _attempts = [];

    private Transmission()
    {
    }

    public Guid Id { get; private set; }

    /// <summary>Référence du document certifié (pas son contenu).</summary>
    public string DocumentReference { get; private set; } = string.Empty;

    public string RecipientSiret { get; private set; } = string.Empty;

    /// <summary>Code service exécutant Chorus Pro, si applicable.</summary>
    public string? ExecutingService { get; private set; }

    public TransmissionChannel Channel { get; private set; }

    public TransmissionStatus Status { get; private set; }

    public string? ChannelDepositId { get; private set; }

    public string? RejectionReason { get; private set; }

    public DateTime CreatedOnUtc { get; private set; }

    public DateTime LastUpdatedOnUtc { get; private set; }

    public IReadOnlyList<TransmissionAttempt> Attempts => _attempts.AsReadOnly();

    public static Result<Transmission> Create(
        string documentReference,
        string recipientSiret,
        TransmissionChannel channel,
        DateTime createdOnUtc,
        string? executingService = null)
    {
        if (string.IsNullOrWhiteSpace(documentReference))
        {
            return Result.Failure<Transmission>(TransmissionErrors.DocumentReferenceRequired);
        }

        if (string.IsNullOrWhiteSpace(recipientSiret))
        {
            return Result.Failure<Transmission>(TransmissionErrors.RecipientSiretRequired);
        }

        return new Transmission
        {
            Id = Guid.CreateVersion7(),
            DocumentReference = documentReference.Trim(),
            RecipientSiret = recipientSiret.Trim(),
            ExecutingService = string.IsNullOrWhiteSpace(executingService) ? null : executingService.Trim(),
            Channel = channel,
            Status = TransmissionStatus.Pending,
            CreatedOnUtc = createdOnUtc,
            LastUpdatedOnUtc = createdOnUtc,
        };
    }

    private bool IsFinalized => Status is TransmissionStatus.Accepted or TransmissionStatus.Rejected;

    public Result RecordSubmission(string channelDepositId, string? rawResponse, DateTime occurredOnUtc)
    {
        if (IsFinalized)
        {
            return Result.Failure(TransmissionErrors.AlreadyFinalized);
        }

        Status = TransmissionStatus.Submitted;
        ChannelDepositId = channelDepositId;
        AddAttempt(TransmissionStatus.Submitted, occurredOnUtc, channelDepositId, rawResponse, null);

        return Result.Success();
    }

    public Result RecordAcceptance(string? rawResponse, DateTime occurredOnUtc)
    {
        if (IsFinalized)
        {
            return Result.Failure(TransmissionErrors.AlreadyFinalized);
        }

        Status = TransmissionStatus.Accepted;
        AddAttempt(TransmissionStatus.Accepted, occurredOnUtc, ChannelDepositId, rawResponse, null);

        return Result.Success();
    }

    public Result RecordRejection(string reason, string? rawResponse, DateTime occurredOnUtc)
    {
        if (IsFinalized)
        {
            return Result.Failure(TransmissionErrors.AlreadyFinalized);
        }

        Status = TransmissionStatus.Rejected;
        RejectionReason = reason;
        AddAttempt(TransmissionStatus.Rejected, occurredOnUtc, ChannelDepositId, rawResponse, reason);

        return Result.Success();
    }

    public void RecordTechnicalFailure(string message, string? rawResponse, DateTime occurredOnUtc)
    {
        Status = TransmissionStatus.TechnicalFailure;
        AddAttempt(TransmissionStatus.TechnicalFailure, occurredOnUtc, ChannelDepositId, rawResponse, message);
    }

    private void AddAttempt(
        TransmissionStatus status,
        DateTime occurredOnUtc,
        string? channelDepositId,
        string? rawResponse,
        string? message)
    {
        _attempts.Add(TransmissionAttempt.Create(Id, status, occurredOnUtc, channelDepositId, rawResponse, message));
        LastUpdatedOnUtc = occurredOnUtc;
    }
}
