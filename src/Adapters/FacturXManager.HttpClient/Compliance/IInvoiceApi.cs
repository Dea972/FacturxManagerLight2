using System;
using System.Collections.Generic;
using System.Text;
using FacturXManager.Client.Compliance.Models;
using Refit;

namespace FacturXManager.Client.Compliance;

/// <summary>
/// Client HTTP pour le module Compliance : validation EN16931 et certification Factur-X.
/// </summary>
public interface IInvoiceApi
{
    /// <summary>
    /// Vérifie la conformité EN16931 d'une facture sans produire de document.
    /// </summary>
    [Post("/compliance/invoices/validate")]
    Task<ComplianceReportDto> ValidateAsync(
        [Body] ValidateInvoiceRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Génère une facture Factur-X : génération du XML CII et du PDF/A-3b embarqué (Factur-X).
    /// </summary>
    [Post("/compliance/invoices/generate")]
    Task<GenerateInvoiceDto> GenerateAsync(
        [Body] GenerateInvoiceRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Génère et télécharge directement le PDF/A-3b Factur-X d'une facture (retour binaire application/pdf).
    /// </summary>
    [Post("/compliance/invoices/download")]
    Task<ApiResponse<Stream>> DownloadInvoicePdfAsync(
        [Body] DownloadInvoiceRequest request,
        CancellationToken cancellationToken = default);


    /// <summary>
    /// Attache les métadonnées et le XML CII Factur-X conformes EN16931 au sein d'un PDF existant.
    /// </summary>
    [Multipart]
    [Post("/compliance/invoices/attach-facturx")]
    Task<AttachFacturXDto> AttachFacturXAsync(
        [AliasAs("file")] StreamPart file,
        [AliasAs("invoice")] string invoiceJson,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retourne la liste des factures certifiées et persistées.
    /// </summary>
    [Get("/compliance/invoices")]
    Task<List<InvoiceSummaryDto>> GetInvoicesAsync(CancellationToken cancellationToken = default);

}
