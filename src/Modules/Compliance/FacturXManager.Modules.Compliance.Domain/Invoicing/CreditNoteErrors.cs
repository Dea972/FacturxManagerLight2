using FacturXManager.Common.Domain;

namespace FacturXManager.Modules.Compliance.Domain.Invoicing;

/// <summary>
/// Erreurs métier liées à l'agrégat <see cref="CreditNote"/>.
/// </summary>
public static class CreditNoteErrors
{
    public static readonly Error NumberRequired =
        Error.Problem("Compliance.CreditNote.NumberRequired", "Le numéro d'avoir est obligatoire.");

    public static readonly Error CurrencyRequired =
        Error.Problem("Compliance.CreditNote.CurrencyRequired", "La devise (code ISO 4217) est obligatoire.");

    public static readonly Error NoLines =
        Error.Problem("Compliance.CreditNote.NoLines", "Un avoir doit comporter au moins une ligne.");

    public static readonly Error SellerRequired =
        Error.Problem("Compliance.CreditNote.SellerRequired", "Les informations du vendeur sont obligatoires.");

    public static readonly Error BuyerRequired =
        Error.Problem("Compliance.CreditNote.BuyerRequired", "Les informations de l'acheteur sont obligatoires.");

    public static readonly Error OriginalInvoiceRequired =
        Error.Problem("Compliance.CreditNote.OriginalInvoiceRequired", "La référence à la facture d'origine est obligatoire.");

    public static readonly Error OriginalInvoiceNumberRequired =
        Error.Problem("Compliance.CreditNote.OriginalInvoiceNumberRequired", "Le numéro de la facture d'origine est obligatoire.");
}
