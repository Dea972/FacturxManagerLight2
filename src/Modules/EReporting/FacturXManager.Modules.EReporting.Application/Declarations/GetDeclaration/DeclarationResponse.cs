namespace FacturXManager.Modules.EReporting.Application.Declarations.GetDeclaration;

public sealed record DeclarationResponse(
    Guid Id,
    string Period,
    string TransactionType,
    string Status,
    decimal TotalTaxableAmount,
    decimal TotalVatAmount,
    string? DgfipSubmissionReference,
    DateTime CreatedOnUtc,
    DateTime? SubmittedOnUtc,
    DateTime? AcceptedOnUtc,
    IReadOnlyList<DeclarationEntryResponse> Entries);

public sealed record DeclarationEntryResponse(
    decimal TaxableAmount,
    decimal VatAmount,
    string CountryCode,
    string OperationNature);
