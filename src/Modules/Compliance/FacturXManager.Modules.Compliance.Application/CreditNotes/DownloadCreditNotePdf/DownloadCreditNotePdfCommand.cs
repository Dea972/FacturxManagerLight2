using FacturXManager.Common.Application.Messaging;
using FacturXManager.Modules.Compliance.Application.Abstractions.CreditNotes;

namespace FacturXManager.Modules.Compliance.Application.CreditNotes.DownloadCreditNotePdf;

/// <summary>
/// Génère le PDF/A-3b Factur-X d'un avoir (type 381) et retourne les octets bruts pour téléchargement direct.
/// </summary>
public sealed record DownloadCreditNotePdfCommand(CreditNoteModel CreditNote) : ICommand<DownloadCreditNotePdfSummary>;
