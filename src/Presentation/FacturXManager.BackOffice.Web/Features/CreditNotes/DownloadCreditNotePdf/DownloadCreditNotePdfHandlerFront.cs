using FacturXManager.BackOffice.Web.Common;
using FacturXManager.BackOffice.Web.Features.Compliance;
using FacturXManager.BackOffice.Web.Features.CreditNotes.ViewModels;
using FacturXManager.Client.Compliance;
using FacturXManager.Client.Compliance.Models;
using MediatR;
using Refit;

namespace FacturXManager.BackOffice.Web.Features.CreditNotes.DownloadCreditNotePdf;

/// <summary>
/// Use case front : génère et récupère le PDF/A-3b Factur-X d'un avoir pour téléchargement.
/// </summary>
public sealed record DownloadCreditNotePdfRequestFront(CreditNoteFormViewModel Form)
    : IRequest<Result<GeneratedPdfViewModel>>;

internal sealed class DownloadCreditNotePdfHandlerFront(
    ICreditNoteApi creditNoteApi,
    ILogger<DownloadCreditNotePdfHandlerFront> logger)
    : IRequestHandler<DownloadCreditNotePdfRequestFront, Result<GeneratedPdfViewModel>>
{
    public async Task<Result<GeneratedPdfViewModel>> Handle(
        DownloadCreditNotePdfRequestFront request,
        CancellationToken cancellationToken)
    {
        try
        {
            var apiRequest = new DownloadCreditNoteRequest
            {
                CreditNote = ComplianceRequestMapper.ToRequest(request.Form),
            };

            using ApiResponse<Stream> response =
                await creditNoteApi.DownloadCreditNotePdfAsync(apiRequest, cancellationToken);

            if (!response.IsSuccessStatusCode || response.Content is null)
            {
                string error = response.Error is { } ex
                    ? ApiErrorTranslator.Describe(ex)
                    : "Le serveur n'a pas retourné de contenu PDF.";
                return Result<GeneratedPdfViewModel>.Failure(error);
            }

            byte[] content = await ReadAllBytesAsync(response.Content, cancellationToken);
            string fileName = ResolveFileName(response, $"avoir-{request.Form.Number}.pdf");

            return Result<GeneratedPdfViewModel>.Success(new GeneratedPdfViewModel(fileName, content));
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Échec du téléchargement du PDF de l'avoir {CreditNoteNumber}", request.Form.Number);
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
