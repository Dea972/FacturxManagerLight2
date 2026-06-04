using FacturXManager.Common.Domain;
using FacturXManager.Modules.Compliance.Application.Invoices;

namespace FacturXManager.Modules.Compliance.Application.Abstractions.Invoices;

/// <summary>
/// Service applicatif de génération d'une facture Factur-X : build → validation EN16931 → XML CII → PDF/A-3b.
/// Factorise la pipeline partagée entre la génération simple et la génération-puis-dépôt.
/// </summary>
public interface IInvoiceService
{
    /// <summary>
    /// Génère une facture Factur-X à partir de son modèle scalaire et retourne l'agrégat + ses artefacts.
    /// </summary>
    Task<Result<CertifiedInvoice>> GenerateAsync(
        InvoiceModel model,
        CancellationToken cancellationToken = default);
}
