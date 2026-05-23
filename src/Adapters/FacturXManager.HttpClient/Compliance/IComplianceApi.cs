using System.Threading;
using System.Threading.Tasks;
using FacturXManager.Client.Compliance.Models;
using Refit;

namespace FacturXManager.Client.Compliance;

/// <summary>
/// Client HTTP pour le module Compliance : validation EN16931 et certification Factur-X.
/// </summary>
public interface IComplianceApi
{
    /// <summary>
    /// Vérifie la conformité EN16931 d'une facture sans produire de document.
    /// </summary>
    [Post("/compliance/invoices/validate")]
    Task<ComplianceReportDto> ValidateAsync(
        [Body] ValidateInvoiceRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Certifie une facture : génération du XML CII et du PDF/A-3b embarqué (Factur-X).
    /// </summary>
    [Post("/compliance/invoices/certify")]
    Task<CertifyInvoiceDto> CertifyAsync(
        [Body] CertifyInvoiceRequest request,
        CancellationToken cancellationToken = default);
}
