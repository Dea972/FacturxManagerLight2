namespace FacturXManager.Modules.EReporting.Application.Declarations.SubmitDeclaration;

public sealed record SubmitDeclarationSummary(
    Guid DeclarationId,
    string Status,
    string SubmissionReference);
