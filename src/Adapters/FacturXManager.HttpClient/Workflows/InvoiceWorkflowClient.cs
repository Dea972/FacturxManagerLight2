using FacturXManager.Client.Compliance;
using FacturXManager.Client.Compliance.Models;
using FacturXManager.Client.Transmission;
using FacturXManager.Client.Transmission.Models;

namespace FacturXManager.Client.Workflows;

/// <summary>
/// Orchestre les appels Compliance + Transmission pour les flux génération + dépôt.
/// Évite les allers-retours côté front : une seule invocation produit et dépose le document.
/// </summary>
public sealed class InvoiceWorkflowClient(
    IInvoiceApi invoiceApi,
    ICreditNoteApi creditNoteApi,
    ITransmissionApi transmissionApi)
{
    /// <summary>
    /// Génère une facture Factur-X (XML CII + PDF/A-3b) puis la dépose sur le canal demandé.
    /// </summary>
    public async Task<GenerateAndSubmitDto> GenerateAndSubmitInvoiceAsync(
        GenerateAndSubmitInvoiceRequest request,
        CancellationToken cancellationToken = default)
    {
        GenerateInvoiceDto invoice = await invoiceApi.GenerateAsync(
            new GenerateInvoiceRequest { Invoice = request.Invoice },
            cancellationToken);

        SubmitTransmissionDto transmission = await transmissionApi.SubmitAsync(
            new SubmitTransmissionRequest
            {
                DocumentReference = invoice.InvoiceNumber,
                RecipientSiret = request.RecipientSiret,
                Channel = request.Channel,
                FacturXPdfBase64 = invoice.FacturXPdfBase64,
                ExecutingService = request.ExecutingService,
            },
            cancellationToken);

        return new GenerateAndSubmitDto
        {
            DocumentId = transmission.TransmissionId,
            DocumentNumber = invoice.InvoiceNumber,
            Profile = invoice.Profile,
            TotalWithVat = invoice.TotalWithVat,
            FacturXPdfBase64 = invoice.FacturXPdfBase64,
            Status = transmission.Status,
            ChannelDepositId = transmission.ChannelDepositId,
            Message = transmission.Message,
        };
    }

    /// <summary>
    /// Génère un avoir Factur-X (XML CII + PDF/A-3b) puis le dépose sur le canal demandé.
    /// </summary>
    public async Task<GenerateAndSubmitDto> GenerateAndSubmitCreditNoteAsync(
        GenerateAndSubmitCreditNoteRequest request,
        CancellationToken cancellationToken = default)
    {
        GenerateCreditNoteDto creditNote = await creditNoteApi.GenerateAsync(
            new GenerateCreditNoteRequest { CreditNote = request.CreditNote },
            cancellationToken);

        SubmitTransmissionDto transmission = await transmissionApi.SubmitAsync(
            new SubmitTransmissionRequest
            {
                DocumentReference = creditNote.CreditNoteNumber,
                RecipientSiret = request.RecipientSiret,
                Channel = request.Channel,
                FacturXPdfBase64 = creditNote.FacturXPdfBase64,
                ExecutingService = request.ExecutingService,
            },
            cancellationToken);

        return new GenerateAndSubmitDto
        {
            DocumentId = transmission.TransmissionId,
            DocumentNumber = creditNote.CreditNoteNumber,
            Profile = creditNote.Profile,
            TotalWithVat = creditNote.TotalWithVat,
            FacturXPdfBase64 = creditNote.FacturXPdfBase64,
            Status = transmission.Status,
            ChannelDepositId = transmission.ChannelDepositId,
            Message = transmission.Message,
        };
    }
}
