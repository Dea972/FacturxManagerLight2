using FacturXManager.Common.Application.Messaging;
using FacturXManager.Common.Domain;
using FacturXManager.Modules.Compliance.Application.Abstractions.CreditNotes;
using FacturXManager.Modules.Compliance.Application.CreditNotes.GenerateCreditNote;
using FacturXManager.Modules.Compliance.Domain.Common;
using FacturXManager.Modules.Compliance.Domain.Invoices;

namespace FacturXManager.Modules.Compliance.Application.CreditNotes.GenerateCreditNoteFromInvoice;

internal sealed class GenerateCreditNoteFromInvoiceCommandHandler(
    IInvoiceRepository invoiceRepository,
    ICreditNoteService creditNoteService)
    : ICommandHandler<GenerateCreditNoteFromInvoiceCommand, GenerateCreditNoteFromInvoiceSummary>
{
    public async Task<Result<GenerateCreditNoteFromInvoiceSummary>> Handle(
        GenerateCreditNoteFromInvoiceCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Charger la facture source.
        Invoice? invoice = await invoiceRepository.GetAsync(request.InvoiceId, cancellationToken);
        if (invoice is null)
        {
            return Result.Failure<GenerateCreditNoteFromInvoiceSummary>(
                InvoiceErrors.NotFound(request.InvoiceId));
        }

        // 2. Orchestrer via CreditNoteService (logique de pré-remplissage).
        var fromInvoiceRequest = new GenerateCreditNoteFromInvoiceRequest(
            invoice,
            request.CreditNoteNumber,
            request.IssueDate,
            request.Lines,
            request.CreditReason);

        Result<GenerateCreditNoteSummary> generateResult =
            await creditNoteService.GenerateFromInvoiceAsync(fromInvoiceRequest, cancellationToken);

        if (generateResult.IsFailure)
        {
            return Result.Failure<GenerateCreditNoteFromInvoiceSummary>(generateResult.Error);
        }

        GenerateCreditNoteSummary summary = generateResult.Value;

        return new GenerateCreditNoteFromInvoiceSummary(
            summary.CreditNoteNumber,
            summary.Profile,
            summary.EmbeddedFileName,
            summary.CrossIndustryInvoiceXml,
            summary.FacturXPdfBase64,
            summary.TotalWithVat);
    }
}
