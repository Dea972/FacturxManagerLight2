using FacturXManager.BackOffice.Web.Common;
using FacturXManager.BackOffice.Web.Features.Compliance;
using FacturXManager.BackOffice.Web.Features.Invoices.ViewModels;
using FacturXManager.Client.Compliance.Models;
using FacturXManager.Client.Workflows;
using MediatR;

namespace FacturXManager.BackOffice.Web.Features.Invoices.GenerateAndSubmitInvoice;

/// <summary>
/// Résultat affiché après génération + dépôt d'une facture.
/// </summary>
public sealed record GenerateAndSubmitInvoiceViewModel(
    Guid InvoiceId,
    string InvoiceNumber,
    string Profile,
    decimal TotalWithVat,
    string FacturXPdfBase64,
    string Status,
    string? ChannelDepositId,
    string? Message);

/// <summary>
/// Use case front : génère une facture Factur-X et la dépose sur Chorus Pro en une étape.
/// </summary>
public sealed record GenerateAndSubmitInvoiceRequestFront(
    InvoiceFormViewModel Form,
    string RecipientSiret,
    string Channel,
    string? ExecutingService = null)
    : IRequest<Result<GenerateAndSubmitInvoiceViewModel>>;

internal sealed class GenerateAndSubmitInvoiceHandlerFront(
    FacturXManagerClient client,
    ILogger<GenerateAndSubmitInvoiceHandlerFront> logger)
    : IRequestHandler<GenerateAndSubmitInvoiceRequestFront, Result<GenerateAndSubmitInvoiceViewModel>>
{
    public async Task<Result<GenerateAndSubmitInvoiceViewModel>> Handle(
        GenerateAndSubmitInvoiceRequestFront request,
        CancellationToken cancellationToken)
    {
        try
        {
            GenerateAndSubmitDto result = await client.Workflows.GenerateAndSubmitInvoiceAsync(
                new GenerateAndSubmitInvoiceRequest
                {
                    Invoice = ComplianceRequestMapper.ToRequest(request.Form),
                    RecipientSiret = request.RecipientSiret,
                    Channel = request.Channel,
                    ExecutingService = request.ExecutingService,
                },
                cancellationToken);

            logger.LogInformation(
                "Facture {DocumentNumber} générée et soumise (profil {Profile}, statut {Status})",
                result.DocumentNumber,
                result.Profile,
                result.Status);

            return Result<GenerateAndSubmitInvoiceViewModel>.Success(new GenerateAndSubmitInvoiceViewModel(
                result.DocumentId,
                result.DocumentNumber,
                result.Profile,
                result.TotalWithVat,
                result.FacturXPdfBase64,
                result.Status,
                result.ChannelDepositId,
                result.Message));
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Échec du traitement de la facture {InvoiceNumber}", request.Form.Number);
            return Result<GenerateAndSubmitInvoiceViewModel>.Failure(ApiErrorTranslator.Describe(exception));
        }
    }
}
