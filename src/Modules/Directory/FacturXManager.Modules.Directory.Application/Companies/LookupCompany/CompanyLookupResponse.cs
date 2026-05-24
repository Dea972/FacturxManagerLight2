namespace FacturXManager.Modules.Directory.Application.Companies.LookupCompany;

public sealed record CompanyLookupResponse(
    string Siren,
    string? Siret,
    string? LegalName,
    string Status,
    bool IsActive,
    bool IsPubliclyDisclosed,
    string Source,
    string? AttachedPlatformIdentifier);
