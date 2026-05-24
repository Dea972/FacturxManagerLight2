using FacturXManager.Common.Application.Messaging;
using FacturXManager.Common.Domain;
using FacturXManager.Modules.Compliance.Application.Abstractions;
using FacturXManager.Modules.Compliance.Domain.Invoicing;

namespace FacturXManager.Modules.Compliance.Application.Invoicing.CertifyInvoice;

internal sealed class CertifyInvoiceCommandHandler(
    IInvoiceBuilder invoiceBuilder,
    IEn16931Validator validator,
    ICrossIndustryInvoiceWriter xmlWriter,
    IFacturXPdfA3BRenderer pdfRenderer)
    : ICommandHandler<CertifyInvoiceCommand, CertifyInvoiceSummary>
{
    public async Task<Result<CertifyInvoiceSummary>> Handle(
        CertifyInvoiceCommand request,
        CancellationToken cancellationToken)
    {
        Result<Invoice> buildResult = invoiceBuilder.Build(request.Invoice);
        if (buildResult.IsFailure)
        {
            return Result.Failure<CertifyInvoiceSummary>(buildResult.Error);
        }

        Invoice invoice = buildResult.Value;

        Result validation = validator.Validate(invoice);
        if (validation.IsFailure)
        {
            return Result.Failure<CertifyInvoiceSummary>(validation.Error);
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
            invoice.Profile);

        Result<byte[]> pdfResult = await pdfRenderer.RenderAsync(renderRequest, cancellationToken);
        if (pdfResult.IsFailure)
        {
            return Result.Failure<CertifyInvoiceSummary>(pdfResult.Error);
        }

        return new CertifyInvoiceSummary(
            invoice.Number,
            invoice.Profile.ToString(),
            FacturXConstants.EmbeddedXmlFileName,
            ciiXml,
            Convert.ToBase64String(pdfResult.Value),
            invoice.TotalWithVat);
    }
}
