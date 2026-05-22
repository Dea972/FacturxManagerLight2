using FacturXManager.Common.Application.Messaging;

namespace FacturXManager.Modules.Transmission.Application.Transmissions.SubmitTransmission;

/// <summary>
/// Dépose un document certifié (référencé) sur une plateforme et trace la tentative.
/// </summary>
public sealed record SubmitTransmissionCommand(
    string DocumentReference,
    string RecipientSiret,
    string Channel,
    string FacturXPdfBase64,
    string? ExecutingService = null) : ICommand<SubmitTransmissionSummary>;
