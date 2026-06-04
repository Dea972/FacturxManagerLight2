using FacturXManager.Common.Application.Messaging;
using FacturXManager.Common.Domain;
using FacturXManager.Modules.Compliance.Domain.Common;
using FacturXManager.Modules.Compliance.Domain.Invoices;

namespace FacturXManager.Modules.Compliance.Application.Invoices.GetInvoices;

internal sealed class GetInvoicesQueryHandler(IInvoiceRepository invoiceRepository)
    : IQueryHandler<GetInvoicesQuery, List<InvoiceSummaryResponse>>
{
    public async Task<Result<List<InvoiceSummaryResponse>>> Handle(
        GetInvoicesQuery request,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<Invoice> invoices = await invoiceRepository.ListAsync(cancellationToken);

        var response = invoices
            .Select(i => new InvoiceSummaryResponse(
                i.Id,
                i.Number,
                i.IssueDate,
                i.CurrencyCode,
                i.Profile.ToString(),
                i.Seller.Name,
                i.Buyer.Name,
                i.TotalWithVat,
                i.Status.ToString(),
                i.ChannelDepositId))
            .ToList();

        return response;
    }
}
