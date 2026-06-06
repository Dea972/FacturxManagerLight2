using FacturXManager.BackOffice.Web.Common;
using FacturXManager.BackOffice.Web.Features.Compliance;
using FacturXManager.BackOffice.Web.Features.Invoices.ViewModels;
using FacturXManager.Client.Compliance;
using FacturXManager.Client.Compliance.Models;
using MediatR;
using Refit;

namespace FacturXManager.BackOffice.Web.Features.Invoices.DownloadInvoicePdf;

/// <summary>
/// Use case front : génère et récupère le PDF/A-3b Factur-X d'une facture pour téléchargement.
/// </summary>
public sealed record DownloadInvoicePdfRequestFront(InvoiceFormViewModel Form)
    : IRequest<Result<GeneratedPdfViewModel>>;

internal sealed class DownloadInvoicePdfHandlerFront(
    IInvoiceApi invoiceApi,
    ILogger<DownloadInvoicePdfHandlerFront> logger)
    : IRequestHandler<DownloadInvoicePdfRequestFront, Result<GeneratedPdfViewModel>>
{
    public async Task<Result<GeneratedPdfViewModel>> Handle(
        DownloadInvoicePdfRequestFront request,
        CancellationToken cancellationToken)
    {
        try
        {
            var apiRequest = new DownloadInvoiceRequest
            {
                Invoice = ComplianceRequestMapper.ToRequest(request.Form),
            };

            using ApiResponse<Stream> response =
                await invoiceApi.DownloadInvoicePdfAsync(apiRequest, cancellationToken);

            if (!response.IsSuccessStatusCode || response.Content is null)
            {
                string error = response.Error is { } ex
                    ? ApiErrorTranslator.Describe(ex)
                    : "Le serveur n'a pas retourné de contenu PDF.";
                return Result<GeneratedPdfViewModel>.Failure(error);
            }

            byte[] content = await ReadAllBytesAsync(response.Content, cancellationToken);
            string fileName = ResolveFileName(response, $"facture-{request.Form.Number}.pdf");

            return Result<GeneratedPdfViewModel>.Success(new GeneratedPdfViewModel(fileName, content));
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Échec du téléchargement du PDF de la facture {InvoiceNumber}", request.Form.Number);
            return Result<GeneratedPdfViewModel>.Failure(ApiErrorTranslator.Describe(exception));
        }
    }

    private static async Task<byte[]> ReadAllBytesAsync(Stream stream, CancellationToken cancellationToken)
    {
        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory, cancellationToken);
        return memory.ToArray();
    }

    private static string ResolveFileName(ApiResponse<Stream> response, string fallback) =>
        response.ContentHeaders?.ContentDisposition?.FileNameStar
        ?? response.ContentHeaders?.ContentDisposition?.FileName?.Trim('"')
        ?? fallback;
}
