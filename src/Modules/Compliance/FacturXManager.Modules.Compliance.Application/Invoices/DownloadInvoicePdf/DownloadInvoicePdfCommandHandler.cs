using FacturXManager.Common.Application.Messaging;
using FacturXManager.Common.Domain;
using FacturXManager.Modules.Compliance.Application.Abstractions;
using FacturXManager.Modules.Compliance.Application.Abstractions.Invoices;

namespace FacturXManager.Modules.Compliance.Application.Invoices.DownloadInvoicePdf;

internal sealed class DownloadInvoicePdfCommandHandler(
    IInvoiceService certificationService,
    IFacturXMetadataExtractor metadataExtractor)
    : ICommandHandler<DownloadInvoicePdfCommand, DownloadInvoicePdfSummary>
{
    public async Task<Result<DownloadInvoicePdfSummary>> Handle(
        DownloadInvoicePdfCommand request,
        CancellationToken cancellationToken)
    {
        Result<CertifiedInvoice> result =
            await certificationService.GenerateAsync(request.Invoice, cancellationToken);

        if (result.IsFailure)
        {
            return Result.Failure<DownloadInvoicePdfSummary>(result.Error);
        }

        CertifiedInvoice certified = result.Value;
        string fileName = $"facturx-{certified.Invoice.Number}.pdf";
        string xmp = metadataExtractor.ExtractXmp(certified.PdfBytes);

        return new DownloadInvoicePdfSummary(certified.PdfBytes, fileName, certified.CrossIndustryInvoiceXml, xmp);
    }
}
