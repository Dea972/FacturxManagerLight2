using FacturXManager.Common.Domain;

namespace FacturXManager.Modules.Transmission.Domain.Transmissions;

public static class TransmissionErrors
{
    public static readonly Error DocumentReferenceRequired =
        Error.Problem(
            "Transmission.DocumentReferenceRequired",
            "La référence du document certifié est obligatoire.");

    public static readonly Error RecipientSiretRequired =
        Error.Problem("Transmission.RecipientSiretRequired", "Le SIRET du destinataire est obligatoire.");

    public static Error InvalidChannel(string value) =>
        Error.Problem("Transmission.InvalidChannel", $"Le canal d'acheminement '{value}' est invalide.");

    public static Error NotFound(Guid transmissionId) =>
        Error.NotFound("Transmission.NotFound", $"La transmission {transmissionId} est introuvable.");

    public static readonly Error AlreadyFinalized =
        Error.Conflict(
            "Transmission.AlreadyFinalized",
            "La transmission est déjà finalisée (acceptée ou rejetée) et ne peut plus changer d'état.");

    public static Error Rejected(string motif) =>
        Error.Problem("Transmission.Rejected", $"Dépôt rejeté par la plateforme : {motif}");

    public static Error TechnicalFailure(string message) =>
        Error.ExternalService("Transmission.ExternalService", $"Erreur technique du canal de transmission : {message}");

    public static Error Timeout(string message) =>
        Error.Timeout("Transmission.Timeout", $"Le délai d'attente du canal a été dépassé : {message}");
}
