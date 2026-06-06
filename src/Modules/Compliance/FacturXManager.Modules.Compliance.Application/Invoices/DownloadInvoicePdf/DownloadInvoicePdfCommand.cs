using FacturXManager.Common.Application.Messaging;
using FacturXManager.Modules.Compliance.Application.Abstractions.Invoices;

namespace FacturXManager.Modules.Compliance.Application.Invoices.DownloadInvoicePdf;

/// <summary>
/// Génère le PDF/A-3b Factur-X et retourne les octets bruts pour téléchargement direct.
/// </summary>
public sealed record DownloadInvoicePdfCommand(InvoiceModel Invoice) : ICommand<DownloadInvoicePdfSummary>;
