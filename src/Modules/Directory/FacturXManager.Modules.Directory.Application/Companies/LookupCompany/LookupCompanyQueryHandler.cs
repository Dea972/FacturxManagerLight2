using FacturXManager.Common.Application.Messaging;
using FacturXManager.Common.Domain;
using FacturXManager.Modules.Directory.Application.Abstractions;
using FacturXManager.Modules.Directory.Domain.Companies;

namespace FacturXManager.Modules.Directory.Application.Companies.LookupCompany;

internal sealed class LookupCompanyQueryHandler(IDirectoryLookupProvider lookupProvider)
    : IQueryHandler<LookupCompanyQuery, CompanyLookupResponse>
{
    public async Task<Result<CompanyLookupResponse>> Handle(
        LookupCompanyQuery request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Identifier))
        {
            return Result.Failure<CompanyLookupResponse>(CompanyErrors.IdentifierRequired);
        }

        // Tolère espaces et tirets en entrée (saisie utilisateur).
        string normalized = StripWhitespaceAndDashes(request.Identifier);

        Result<CompanyDirectoryEntry> lookupResult = normalized.Length switch
        {
            Siren.Length => await LookupBySirenAsync(normalized, cancellationToken),
            Siret.Length => await LookupBySiretAsync(normalized, cancellationToken),
            _ => Result.Failure<CompanyDirectoryEntry>(CompanyErrors.UnknownIdentifierFormat),
        };

        if (lookupResult.IsFailure)
        {
            return Result.Failure<CompanyLookupResponse>(lookupResult.Error);
        }

        return Map(lookupResult.Value);
    }

    private async Task<Result<CompanyDirectoryEntry>> LookupBySirenAsync(
        string normalized,
        CancellationToken cancellationToken)
    {
        Result<Siren> sirenResult = Siren.Create(normalized);
        if (sirenResult.IsFailure)
        {
            return Result.Failure<CompanyDirectoryEntry>(sirenResult.Error);
        }

        return await lookupProvider.LookupBySirenAsync(sirenResult.Value, cancellationToken);
    }

    private async Task<Result<CompanyDirectoryEntry>> LookupBySiretAsync(
        string normalized,
        CancellationToken cancellationToken)
    {
        Result<Siret> siretResult = Siret.Create(normalized);
        if (siretResult.IsFailure)
        {
            return Result.Failure<CompanyDirectoryEntry>(siretResult.Error);
        }

        return await lookupProvider.LookupBySiretAsync(siretResult.Value, cancellationToken);
    }

    private static CompanyLookupResponse Map(CompanyDirectoryEntry entry) =>
        new(
            entry.Siren.Value,
            entry.Siret?.Value,
            entry.LegalName,
            entry.Status.ToString(),
            entry.IsActive,
            entry.IsPubliclyDisclosed,
            entry.Source.ToString(),
            entry.AttachedPlatformIdentifier);

    private static string StripWhitespaceAndDashes(string raw) =>
        new(raw.Where(c => !char.IsWhiteSpace(c) && c != '-').ToArray());
}
