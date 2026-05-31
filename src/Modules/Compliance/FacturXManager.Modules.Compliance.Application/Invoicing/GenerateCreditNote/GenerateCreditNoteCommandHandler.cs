using FacturXManager.Common.Application.Messaging;
using FacturXManager.Common.Domain;
using FacturXManager.Modules.Compliance.Application.Abstractions;
using FacturXManager.Modules.Compliance.Domain.Invoicing;

namespace FacturXManager.Modules.Compliance.Application.Invoicing.GenerateCreditNote;

internal sealed class GenerateCreditNoteCommandHandler(
    ICreditNoteBuilder creditNoteBuilder,
    ICrossIndustryInvoiceWriter xmlWriter,
    IFacturXPdfA3BRenderer pdfRenderer)
    : ICommandHandler<GenerateCreditNoteCommand, GenerateCreditNoteSummary>
{
    public async Task<Result<GenerateCreditNoteSummary>> Handle(
        GenerateCreditNoteCommand request,
        CancellationToken cancellationToken)
    {
        Result<CreditNote> buildResult = creditNoteBuilder.Build(request.CreditNote);
        if (buildResult.IsFailure)
        {
            return Result.Failure<GenerateCreditNoteSummary>(buildResult.Error);
        }

        CreditNote creditNote = buildResult.Value;

        string ciiXml = xmlWriter.Write(creditNote);

        var renderRequest = new FacturXRenderRequest(
            ciiXml,
            FacturXConstants.EmbeddedXmlFileName,
            creditNote.Number,
            creditNote.Seller.Name,
            creditNote.Buyer.Name,
            creditNote.IssueDate,
            creditNote.CurrencyCode,
            creditNote.TotalWithoutVat,
            creditNote.TotalVat,
            creditNote.TotalWithVat,
            creditNote.Profile,
            SellerAddress: FormatAddress(creditNote.Seller.Address),
            SellerVatIdentifier: creditNote.Seller.VatIdentifier,
            SellerLegalRegistrationId: creditNote.Seller.LegalRegistrationId,
            BuyerAddress: FormatAddress(creditNote.Buyer.Address),
            BuyerVatIdentifier: creditNote.Buyer.VatIdentifier,
            BuyerLegalRegistrationId: creditNote.Buyer.LegalRegistrationId,
            AmountDue: creditNote.AmountDue,
            Lines: creditNote.Lines.Select(l => new RenderLineItem(
                l.Id, l.Description, l.Quantity, l.UnitCode, l.UnitNetPrice, l.VatRate, l.NetAmount)).ToList(),
            VatBreakdowns: creditNote.VatBreakdowns.Select(v => new RenderVatBreakdown(
                v.Category.ToString(), v.Rate, v.BasisAmount, v.TaxAmount)).ToList());

        Result<byte[]> pdfResult = await pdfRenderer.RenderAsync(renderRequest, cancellationToken);
        if (pdfResult.IsFailure)
        {
            return Result.Failure<GenerateCreditNoteSummary>(pdfResult.Error);
        }

        return new GenerateCreditNoteSummary(
            creditNote.Number,
            creditNote.Profile.ToString(),
            FacturXConstants.EmbeddedXmlFileName,
            ciiXml,
            Convert.ToBase64String(pdfResult.Value),
            creditNote.TotalWithVat);
    }

    private static string FormatAddress(PostalAddress address) =>
        string.IsNullOrWhiteSpace(address.Line2)
            ? $"{address.Line1}, {address.PostalCode} {address.City}, {address.CountryCode}"
            : $"{address.Line1}, {address.Line2}, {address.PostalCode} {address.City}, {address.CountryCode}";
}
