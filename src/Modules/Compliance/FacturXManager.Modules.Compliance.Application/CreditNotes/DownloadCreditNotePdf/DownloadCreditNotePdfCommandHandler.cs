using FacturXManager.Common.Application.Messaging;
using FacturXManager.Common.Domain;
using FacturXManager.Modules.Compliance.Application.Abstractions.CreditNotes;

namespace FacturXManager.Modules.Compliance.Application.CreditNotes.DownloadCreditNotePdf;

internal sealed class DownloadCreditNotePdfCommandHandler(
    ICreditNoteService certificationService)
    : ICommandHandler<DownloadCreditNotePdfCommand, DownloadCreditNotePdfSummary>
{
    public async Task<Result<DownloadCreditNotePdfSummary>> Handle(
        DownloadCreditNotePdfCommand request,
        CancellationToken cancellationToken)
    {
        Result<CertifiedCreditNote> result =
            await certificationService.GenerateAsync(request.CreditNote, cancellationToken);

        if (result.IsFailure)
        {
            return Result.Failure<DownloadCreditNotePdfSummary>(result.Error);
        }

        CertifiedCreditNote certified = result.Value;
        string fileName = $"avoir-{certified.CreditNote.Number}.pdf";

        return new DownloadCreditNotePdfSummary(certified.PdfBytes, fileName, certified.CrossIndustryInvoiceXml);
    }
}
