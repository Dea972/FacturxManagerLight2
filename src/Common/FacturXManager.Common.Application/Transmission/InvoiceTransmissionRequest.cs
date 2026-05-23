namespace FacturXManager.Common.Application.Transmission;

/// <summary>
/// Demande d'acheminement, neutre vis-à-vis du canal. Le contenu PDF n'est pas persisté
/// par le module de transmission ; il n'est utilisé que pour le dépôt.
/// </summary>
public sealed record InvoiceTransmissionRequest(
    TransmissionChannel Channel,
    string DocumentReference,
    string RecipientSiret,
    string FacturXPdfBase64,
    string? ExecutingService = null);
