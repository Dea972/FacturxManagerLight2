using FacturXManager.Common.Application.Messaging;
using FacturXManager.Modules.Compliance.Application.Abstractions;

namespace FacturXManager.Modules.Compliance.Application.Invoicing.DownloadCreditNotePdf;

/// <summary>
/// Génère le PDF/A-3b Factur-X d'un avoir (type 381) et retourne les octets bruts pour téléchargement direct.
/// </summary>
public sealed record DownloadCreditNotePdfCommand(CreditNoteModel CreditNote) : ICommand<DownloadCreditNotePdfSummary>;
