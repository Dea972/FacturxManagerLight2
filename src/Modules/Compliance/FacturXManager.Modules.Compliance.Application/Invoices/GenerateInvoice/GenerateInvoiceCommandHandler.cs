using FacturXManager.Common.Application.Messaging;
using FacturXManager.Common.Domain;
using FacturXManager.Modules.Compliance.Application.Abstractions.Invoices;
using FacturXManager.Modules.Compliance.Domain.Common;
using FacturXManager.Modules.Compliance.Domain.Invoices;

namespace FacturXManager.Modules.Compliance.Application.Invoices.GenerateInvoice;

internal sealed class GenerateInvoiceCommandHandler(IInvoiceService certificationService)
    : ICommandHandler<GenerateInvoiceCommand, GenerateInvoiceSummary>
{
    public async Task<Result<GenerateInvoiceSummary>> Handle(
        GenerateInvoiceCommand request,
        CancellationToken cancellationToken)
    {
        Result<CertifiedInvoice> result =
            await certificationService.GenerateAsync(request.Invoice, cancellationToken);

        if (result.IsFailure)
        {
            return Result.Failure<GenerateInvoiceSummary>(result.Error);
        }

        CertifiedInvoice certified = result.Value;
        Invoice invoice = certified.Invoice;

        return new GenerateInvoiceSummary(
            invoice.Number,
            invoice.Profile.ToString(),
            FacturXConstants.EmbeddedXmlFileName,
            certified.CrossIndustryInvoiceXml,
            Convert.ToBase64String(certified.PdfBytes),
            invoice.TotalWithVat);
    }
}
