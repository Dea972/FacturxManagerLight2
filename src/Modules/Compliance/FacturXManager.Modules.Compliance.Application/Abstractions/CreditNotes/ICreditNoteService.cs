using FacturXManager.Common.Domain;
using FacturXManager.Modules.Compliance.Application.CreditNotes;
using FacturXManager.Modules.Compliance.Application.CreditNotes.GenerateCreditNote;

namespace FacturXManager.Modules.Compliance.Application.Abstractions.CreditNotes;

public interface ICreditNoteService
{
    /// <summary>
    /// Génère un avoir Factur-X à partir d'une facture existante et des paramètres de l'avoir.
    /// </summary>
    Task<Result<GenerateCreditNoteSummary>> GenerateFromInvoiceAsync(
        GenerateCreditNoteFromInvoiceRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Génère un avoir à partir de son modèle scalaire et retourne l'agrégat + ses artefacts.
    /// </summary>
    Task<Result<CertifiedCreditNote>> GenerateAsync(
        CreditNoteModel model,
        CancellationToken cancellationToken);

}
