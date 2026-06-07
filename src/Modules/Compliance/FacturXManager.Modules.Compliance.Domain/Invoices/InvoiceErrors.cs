using FacturXManager.Common.Domain;

namespace FacturXManager.Modules.Compliance.Domain.Invoices;

public static class InvoiceErrors
{
    public static readonly Error NumberRequired =
        Error.Problem("Compliance.Invoice.NumberRequired", "Le numéro de facture est obligatoire.");

    public static readonly Error CurrencyRequired =
        Error.Problem("Compliance.Invoice.CurrencyRequired", "La devise (code ISO 4217) est obligatoire.");

    public static readonly Error NoLines =
        Error.Problem("Compliance.Invoice.NoLines", "Une facture doit comporter au moins une ligne.");

    public static readonly Error SellerRequired =
        Error.Problem("Compliance.Invoice.SellerRequired", "Les informations du vendeur sont obligatoires.");

    public static readonly Error BuyerRequired =
        Error.Problem("Compliance.Invoice.BuyerRequired", "Les informations de l'acheteur sont obligatoires.");

    public static readonly Error SellerSirenRequired =
        Error.Problem("Compliance.Invoice.SellerSirenRequired", "Le SIREN/SIRET du vendeur est obligatoire.");

    public static readonly Error BuyerSirenRequired =
        Error.Problem(
            "Compliance.Invoice.BuyerSirenRequired",
            "Le SIREN de l'acheteur est obligatoire pour toute facture B2B émise à partir du 1er septembre 2026.");

    public static readonly Error OperationNatureRequired =
        Error.Problem(
            "Compliance.Invoice.OperationNatureRequired",
            "La nature de l'opération est obligatoire pour toute facture émise à partir du 1er septembre 2026.");

    public static readonly Error SellerVatIdentifierRequired =
        Error.Problem(
            "Compliance.Invoice.SellerVatIdentifierRequired",
            "Le numéro de TVA du vendeur est obligatoire pour le profil EN16931.");

    public static Error LineDescriptionRequired(string lineId) =>
        Error.Problem(
            "Compliance.Invoice.LineDescriptionRequired",
            $"La ligne '{lineId}' doit comporter une désignation.");

    public static Error LineQuantityInvalid(string lineId) =>
        Error.Problem(
            "Compliance.Invoice.LineQuantityInvalid",
            $"La quantité de la ligne '{lineId}' doit être strictement positive.");

    public static Error PositiveRateForExemptCategory(string category) =>
        Error.Problem(
            "Compliance.Invoice.PositiveRateForExemptCategory",
            $"Un taux de TVA strictement positif est incohérent avec la catégorie '{category}'.");

    public static Error TotalsMismatch(decimal expected, decimal actual) =>
        Error.Problem(
            "Compliance.Invoice.TotalsMismatch",
            $"Le total TTC calculé ({actual}) ne correspond pas au total attendu ({expected}).");

    public static Error InvalidProfile(string value) =>
        Error.Problem("Compliance.Invoice.InvalidProfile", $"Le profil Factur-X '{value}' est invalide.");

    public static Error InvalidOperationNature(string value) =>
        Error.Problem(
            "Compliance.Invoice.InvalidOperationNature",
            $"La nature d'opération '{value}' est invalide.");

    public static Error InvalidVatCategory(string value) =>
        Error.Problem("Compliance.Invoice.InvalidVatCategory", $"La catégorie de TVA '{value}' est invalide.");

    public static readonly Error PdfRendererNotConfigured =
        Error.Problem(
            "Compliance.PdfRendererNotConfigured",
            "Aucun moteur de rendu PDF/A-3b n'est configuré. Enregistrez une implémentation de IFacturXPdfA3bRenderer.");

    public static readonly Error InvalidPdf =
        Error.Problem("Compliance.InvalidPdf", "Le fichier fourni n'est pas un PDF valide ou est vide.");

    public static readonly Error PdfEmbedFailed =
        Error.Problem(
            "Compliance.PdfEmbedFailed",
            "L'intégration des données Factur-X dans le PDF fourni a échoué.");

    public static Error InvalidTransmissionChannel(string value) =>
        Error.Problem(
            "Compliance.Invoice.InvalidTransmissionChannel",
            $"Le canal de transmission '{value}' est invalide.");

    public static Error TransmissionRejected(string message) =>
        Error.Problem(
            "Compliance.Invoice.TransmissionRejected",
            $"La plateforme a rejeté le dépôt : {message}");

    public static Error TransmissionFailed(string message) =>
        Error.Problem(
            "Compliance.Invoice.TransmissionFailed",
            $"Échec technique du dépôt : {message}");

    public static Error NotFound(Guid id) =>
        Error.NotFound("Compliance.Invoice.NotFound", $"La facture '{id}' est introuvable.");
}
