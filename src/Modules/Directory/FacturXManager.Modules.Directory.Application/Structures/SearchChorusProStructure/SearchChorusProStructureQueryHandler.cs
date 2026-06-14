using FacturXManager.Common.Application.Messaging;
using FacturXManager.Common.Domain;
using FacturXManager.Modules.Directory.Application.Abstractions;
using FacturXManager.Modules.Directory.Application.Companies.LookupCompany;
using FacturXManager.Modules.Directory.Domain.Companies;

namespace FacturXManager.Modules.Directory.Application.Structures.SearchChorusProStructure;

internal sealed class SearchChorusProStructureQueryHandler(IChorusProStructureService structureService)
    : IQueryHandler<SearchChorusProStructureQuery, CompanyLookupResponse>
{
    public async Task<Result<CompanyLookupResponse>> Handle(
        SearchChorusProStructureQuery request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Siret))
        {
            return Result.Failure<CompanyLookupResponse>(CompanyErrors.IdentifierRequired);
        }

        string normalized = new(request.Siret.Where(c => !char.IsWhiteSpace(c) && c != '-').ToArray());

        if (normalized.Length != Siret.Length)
        {
            return Result.Failure<CompanyLookupResponse>(CompanyErrors.InvalidSiretFormat);
        }

        return await structureService.SearchBySiretAsync(normalized, cancellationToken);
    }
}
