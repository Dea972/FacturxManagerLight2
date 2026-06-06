using FacturXManager.Common.Application.Messaging;
using FacturXManager.Modules.Transmission.Application.Transmissions.SubmitTransmission;

namespace FacturXManager.Modules.Transmission.Application.Transmissions.SubmitTransmissionFile;

/// <summary>
/// Dépose un fichier PDF Factur-X directement (octets bruts convertis en Base64) sur la plateforme demandée.
/// </summary>
public sealed record SubmitTransmissionFileCommand(
    string DocumentReference,
    string RecipientSiret,
    string Channel,
    byte[] FileBytes,
    string? ExecutingService = null) : ICommand<SubmitTransmissionSummary>;
