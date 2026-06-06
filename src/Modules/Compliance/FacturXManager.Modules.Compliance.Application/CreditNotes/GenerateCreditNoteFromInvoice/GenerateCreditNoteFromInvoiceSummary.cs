using FacturXManager.Modules.Compliance.Application.CreditNotes.GenerateCreditNote;

namespace FacturXManager.Modules.Compliance.Application.CreditNotes.GenerateCreditNoteFromInvoice;

/// <summary>
/// Résultat de la génération d'un avoir depuis une facture existante.
/// Identique à <see cref="GenerateCreditNoteSummary"/> — réexposé pour la clarté de l'use case.
/// </summary>
public sealed record GenerateCreditNoteFromInvoiceSummary(
    string CreditNoteNumber,
    string Profile,
    string EmbeddedFileName,
    string CrossIndustryInvoiceXml,
    string FacturXPdfBase64,
    decimal TotalWithVat);
