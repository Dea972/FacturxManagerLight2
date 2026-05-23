using FacturXManager.Common.Application.Messaging;
using FacturXManager.Common.Domain;
using FacturXManager.Modules.EReporting.Domain.Declarations;

namespace FacturXManager.Modules.EReporting.Application.Declarations.GetDeclaration;

internal sealed class GetDeclarationQueryHandler(IDeclarationRepository repository)
    : IQueryHandler<GetDeclarationQuery, DeclarationResponse>
{
    public async Task<Result<DeclarationResponse>> Handle(
        GetDeclarationQuery request,
        CancellationToken cancellationToken)
    {
        Declaration? declaration = await repository.GetAsync(request.DeclarationId, cancellationToken);
        if (declaration is null)
        {
            return Result.Failure<DeclarationResponse>(DeclarationErrors.NotFound(request.DeclarationId));
        }

        IReadOnlyList<DeclarationEntryResponse> entries = declaration.Entries
            .Select(e => new DeclarationEntryResponse(
                e.TaxableAmount,
                e.VatAmount,
                e.CountryCode,
                e.OperationNature.ToString()))
            .ToList();

        return new DeclarationResponse(
            declaration.Id,
            declaration.Period.Label,
            declaration.TransactionType.ToString(),
            declaration.Status.ToString(),
            declaration.TotalTaxableAmount,
            declaration.TotalVatAmount,
            declaration.DgfipSubmissionReference,
            declaration.CreatedOnUtc,
            declaration.SubmittedOnUtc,
            declaration.AcceptedOnUtc,
            entries);
    }
}
