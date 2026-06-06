using FacturXManager.BackOffice.Web.Common;
using FacturXManager.Client.Compliance;
using FacturXManager.Client.Compliance.Models;
using MediatR;

namespace FacturXManager.BackOffice.Web.Features.Invoices.GetInvoices;

/// <summary>
/// ViewModel d'une ligne de facture dans la liste.
/// </summary>
public sealed record InvoiceListItemViewModel(
    Guid Id,
    string Number,
    string IssueDate,
    string SellerName,
    string BuyerName,
    decimal TotalWithVat,
    string Status,
    string? ChannelDepositId);

/// <summary>
/// Use case front : charge la liste des factures persistées.
/// </summary>
public sealed record GetInvoicesRequestFront : IRequest<Result<IReadOnlyList<InvoiceListItemViewModel>>>;

internal sealed class GetInvoicesHandlerFront(
    IInvoiceApi invoiceApi,
    ILogger<GetInvoicesHandlerFront> logger)
    : IRequestHandler<GetInvoicesRequestFront, Result<IReadOnlyList<InvoiceListItemViewModel>>>
{
    public async Task<Result<IReadOnlyList<InvoiceListItemViewModel>>> Handle(
        GetInvoicesRequestFront request,
        CancellationToken cancellationToken)
    {
        try
        {
            List<InvoiceSummaryDto> dtos = await invoiceApi.GetInvoicesAsync(cancellationToken);

            IReadOnlyList<InvoiceListItemViewModel> viewModels = dtos
                .Select(d => new InvoiceListItemViewModel(
                    d.Id,
                    d.Number,
                    d.IssueDate,
                    d.SellerName,
                    d.BuyerName,
                    d.TotalWithVat,
                    d.Status,
                    d.ChannelDepositId))
                .ToList();

            return Result<IReadOnlyList<InvoiceListItemViewModel>>.Success(viewModels);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Échec du chargement de la liste des factures.");
            return Result<IReadOnlyList<InvoiceListItemViewModel>>.Failure(ApiErrorTranslator.Describe(exception));
        }
    }
}
