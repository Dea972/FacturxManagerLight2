using FacturXManager.Common.Application.Messaging;
using FacturXManager.Common.Domain;
using FacturXManager.Modules.Compliance.Application.Abstractions;
using FacturXManager.Modules.Compliance.Domain.Invoicing;

namespace FacturXManager.Modules.Compliance.Application.Invoicing.DownloadInvoicePdf;

internal sealed class DownloadInvoicePdfCommandHandler(
    IInvoiceBuilder invoiceBuilder,
    IEn16931Validator validator,
    ICrossIndustryInvoiceWriter xmlWriter,
    IFacturXPdfA3BRenderer pdfRenderer,
    IFacturXMetadataExtractor metadataExtractor)
    : ICommandHandler<DownloadInvoicePdfCommand, DownloadInvoicePdfSummary>
{
    public async Task<Result<DownloadInvoicePdfSummary>> Handle(
        DownloadInvoicePdfCommand request,
        CancellationToken cancellationToken)
    {
        Result<Invoice> buildResult = invoiceBuilder.Build(request.Invoice);
        if (buildResult.IsFailure)
        {
            return Result.Failure<DownloadInvoicePdfSummary>(buildResult.Error);
        }

        Invoice invoice = buildResult.Value;

        Result validation = validator.Validate(invoice);
        if (validation.IsFailure)
        {
            return Result.Failure<DownloadInvoicePdfSummary>(validation.Error);
        }

        string ciiXml = xmlWriter.Write(invoice);

        var renderRequest = new FacturXRenderRequest(
            ciiXml,
            FacturXConstants.EmbeddedXmlFileName,
            invoice.Number,
            invoice.Seller.Name,
            invoice.Buyer.Name,
            invoice.IssueDate,
            invoice.CurrencyCode,
            invoice.TotalWithoutVat,
            invoice.TotalVat,
            invoice.TotalWithVat,
            invoice.Profile,
            SellerAddress: FormatAddress(invoice.Seller.Address),
            SellerVatIdentifier: invoice.Seller.VatIdentifier,
            SellerLegalRegistrationId: invoice.Seller.LegalRegistrationId,
            BuyerAddress: FormatAddress(invoice.Buyer.Address),
            BuyerVatIdentifier: invoice.Buyer.VatIdentifier,
            BuyerLegalRegistrationId: invoice.Buyer.LegalRegistrationId,
            AmountDue: invoice.AmountDue,
            PaymentMeansCode: invoice.Payment.MeansCode,
            PaymentIban: invoice.Payment.Iban,
            PaymentDueDate: invoice.Payment.DueDate,
            Lines: invoice.Lines.Select(l => new RenderLineItem(
                l.Id, l.Description, l.Quantity, l.UnitCode, l.UnitNetPrice, l.VatRate, l.NetAmount)).ToList(),
            VatBreakdowns: invoice.VatBreakdowns.Select(v => new RenderVatBreakdown(
                v.Category.ToString(), v.Rate, v.BasisAmount, v.TaxAmount)).ToList());

        Result<byte[]> pdfResult = await pdfRenderer.RenderAsync(renderRequest, cancellationToken);
        if (pdfResult.IsFailure)
        {
            return Result.Failure<DownloadInvoicePdfSummary>(pdfResult.Error);
        }

        string fileName = $"facturx-{invoice.Number}.pdf";

        string xmp = metadataExtractor.ExtractXmp(pdfResult.Value);

        return new DownloadInvoicePdfSummary(pdfResult.Value, fileName, ciiXml, xmp);
    }

    private static string FormatAddress(PostalAddress address) =>
        string.IsNullOrWhiteSpace(address.Line2)
            ? $"{address.Line1}, {address.PostalCode} {address.City}, {address.CountryCode}"
            : $"{address.Line1}, {address.Line2}, {address.PostalCode} {address.City}, {address.CountryCode}";
}
