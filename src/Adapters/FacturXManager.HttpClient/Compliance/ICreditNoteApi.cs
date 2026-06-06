using System;
using System.Collections.Generic;
using System.Text;
using FacturXManager.Client.Compliance.Models;
using Refit;

namespace FacturXManager.Client.Compliance;

/// <summary>
/// Client HTTP dédié aux opérations sur les avoirs (Credit Notes) : génération de CII type 381 et PDF Factur-X associés.
/// </summary>
public interface ICreditNoteApi
{   
        /// <summary>
    /// Génère et télécharge directement le PDF/A-3b Factur-X d'un avoir (retour binaire application/pdf).
    /// </summary>
    [Post("/compliance/credit-notes/download")]
    Task<ApiResponse<Stream>> DownloadCreditNotePdfAsync(
        [Body] DownloadCreditNoteRequest request,
        CancellationToken cancellationToken = default);


    /// <summary>
    /// Génère un avoir Factur-X (type CII 381) : XML CII avec InvoiceReferencedDocument et PDF/A-3b en Base64.
    /// </summary>
    [Post("/compliance/credit-notes/generate")]
    Task<GenerateCreditNoteDto> GenerateAsync(
        [Body] GenerateCreditNoteRequest request,
        CancellationToken cancellationToken = default);
}
