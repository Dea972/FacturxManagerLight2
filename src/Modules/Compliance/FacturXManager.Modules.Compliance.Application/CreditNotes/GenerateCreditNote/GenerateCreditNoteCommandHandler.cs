using FacturXManager.Common.Application.Messaging;
using FacturXManager.Common.Domain;
using FacturXManager.Modules.Compliance.Application.Abstractions.CreditNotes;
using FacturXManager.Modules.Compliance.Domain.Common;
using FacturXManager.Modules.Compliance.Domain.CreditNotes;

namespace FacturXManager.Modules.Compliance.Application.CreditNotes.GenerateCreditNote;

internal sealed class GenerateCreditNoteCommandHandler(ICreditNoteService certificationService)
    : ICommandHandler<GenerateCreditNoteCommand, GenerateCreditNoteSummary>
{
    public async Task<Result<GenerateCreditNoteSummary>> Handle(
        GenerateCreditNoteCommand request,
        CancellationToken cancellationToken)
    {
        Result<CertifiedCreditNote> result =
            await certificationService.GenerateAsync(request.CreditNote, cancellationToken);

        if (result.IsFailure)
        {
            return Result.Failure<GenerateCreditNoteSummary>(result.Error);
        }

        CertifiedCreditNote certified = result.Value;

        CreditNote creditNote = certified.CreditNote;

        return new GenerateCreditNoteSummary(
            creditNote.Number,
            creditNote.Profile.ToString(),
            FacturXConstants.EmbeddedXmlFileName,
            certified.CrossIndustryInvoiceXml,
            Convert.ToBase64String(certified.PdfBytes),
            creditNote.TotalWithVat);
    }
}
