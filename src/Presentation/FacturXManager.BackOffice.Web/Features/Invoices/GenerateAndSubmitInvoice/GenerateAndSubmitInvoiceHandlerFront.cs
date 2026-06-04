using FacturXManager.BackOffice.Web.Common;
using FacturXManager.BackOffice.Web.Features.Compliance;
using FacturXManager.BackOffice.Web.Features.Invoices;
using FacturXManager.BackOffice.Web.Features.Invoices.GenerateInvoice;
using FacturXManager.BackOffice.Web.Features.Invoices.ViewModels;
using FacturXManager.Client.Compliance;
using FacturXManager.Client.Compliance.Models;
using FacturXManager.Client.Transmission;
using FacturXManager.Client.Transmission.Models;
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
    IInvoiceApi invoiceApi,
    ITransmissionApi transmissionApi,
    ILogger<GenerateAndSubmitInvoiceHandlerFront> logger)
    : IRequestHandler<GenerateAndSubmitInvoiceRequestFront, Result<GenerateAndSubmitInvoiceViewModel>>
{
    public async Task<Result<GenerateAndSubmitInvoiceViewModel>> Handle(
        GenerateAndSubmitInvoiceRequestFront request,
        CancellationToken cancellationToken)
    {
        try
        {
            // 1. Génération de la facture Factur-X (XML CII + PDF/A-3b)
            var generateRequest = new Client.Compliance.Models.GenerateInvoiceRequest
            {
                Invoice = ComplianceRequestMapper.ToRequest(request.Form),
            };

            GenerateInvoiceDto dto = await invoiceApi.GenerateAsync(generateRequest, cancellationToken);

            logger.LogInformation(
                "Facture {InvoiceNumber} générée avec succès (profil {Profile})",
                dto.InvoiceNumber,
                dto.Profile);

            // 2. Dépôt sur Chorus Pro via le module Transmission
            var submitRequest = new SubmitTransmissionRequest
            {
                DocumentReference = dto.InvoiceNumber,
                RecipientSiret = request.RecipientSiret,
                Channel = request.Channel,
                FacturXPdfBase64 = dto.FacturXPdfBase64,
            };

            SubmitTransmissionDto transmission = await transmissionApi.SubmitAsync(submitRequest, cancellationToken);

            logger.LogInformation(
                "Facture {InvoiceNumber} soumise sur Chorus Pro — TransmissionId : {TransmissionId}, Statut : {Status}",
                dto.InvoiceNumber,
                transmission.TransmissionId,
                transmission.Status);

            return Result<GenerateAndSubmitInvoiceViewModel>.Success(new GenerateAndSubmitInvoiceViewModel(
                Guid.CreateVersion7(),
                dto.InvoiceNumber,
                dto.Profile,
                dto.TotalWithVat,
                dto.FacturXPdfBase64,
                transmission.Status,
                transmission.ChannelDepositId,
                transmission.Message));
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Échec du traitement de la facture {InvoiceNumber}", request.Form.Number);
            return Result<GenerateAndSubmitInvoiceViewModel>.Failure(ApiErrorTranslator.Describe(exception));
        }
    }
}
