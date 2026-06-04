using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FacturXManager.Common.Domain;
using FacturXManager.Modules.Compliance.Application.Abstractions;
using FacturXManager.Modules.Compliance.Domain.Invoices;
using Microsoft.Extensions.Logging;

namespace FacturXManager.Modules.Compliance.Infrastructure.Invoicing;

/// <summary>
/// Implémentation d'infrastructure pour l'embedding Factur-X dans un PDF existant via PDFSharp.
/// </summary>
internal sealed class FacturXPdfEmbedder(ILogger<FacturXPdfEmbedder> logger) : IFacturXPdfEmbedder
{
    public Task<Result<byte[]>> EmbedAsync(
        byte[] visualPdfBytes,
        FacturXRenderRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (visualPdfBytes == null || visualPdfBytes.Length == 0)
            {
                return Task.FromResult(Result.Failure<byte[]>(InvoiceErrors.InvalidPdf));
            }

            // Utilise l'utilitaire d'embedding PDF existant du projet basé sur PdfSharp
            byte[] embeddedPdf = FacturXPdfEmbedderHelper.EmbedFacturXData(visualPdfBytes, request);

            return Task.FromResult(Result.Success(embeddedPdf));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Échec de l'intégration Factur-X dans le PDF fourni pour la facture {InvoiceNumber}.", request.InvoiceNumber);
            return Task.FromResult(Result.Failure<byte[]>(InvoiceErrors.PdfRendererNotConfigured));
        }
    }
}
