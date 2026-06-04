using FacturXManager.Common.Application.Messaging;
using FacturXManager.Modules.Compliance.Application.Abstractions.CreditNotes;

namespace FacturXManager.Modules.Compliance.Application.CreditNotes.GenerateCreditNoteFromInvoice;

/// <summary>
/// Génère un avoir Factur-X à partir d'une facture persistée, en pré-remplissant
/// les données communes (parties, devise, profil, lignes). Utilise <see cref="CreditNoteService"/>.
/// </summary>
public sealed record GenerateCreditNoteFromInvoiceCommand(
    Guid InvoiceId,
    string CreditNoteNumber,
    DateOnly IssueDate,
    IReadOnlyList<CreditNoteLineModel> Lines,
    string? CreditReason = null)
    : ICommand<GenerateCreditNoteFromInvoiceSummary>;
