using FacturXManager.Common.Domain;

namespace FacturXManager.Modules.EReporting.Application.Abstractions;

/// <summary>
/// Port de soumission d'une déclaration d'e-reporting à la DGFiP.
/// L'implémentation concrète (API DGFiP) est à câbler en Infrastructure.
/// </summary>
public interface IDgfipReportingClient
{
    Task<Result<DgfipSubmissionReceipt>> SubmitAsync(
        DgfipSubmissionRequest request,
        CancellationToken cancellationToken = default);
}

public sealed record DgfipSubmissionRequest(
    string PeriodLabel,
    string TransactionType,
    decimal TotalTaxableAmount,
    decimal TotalVatAmount,
    int EntryCount);

public sealed record DgfipSubmissionReceipt(string SubmissionReference, string? RawResponse);
