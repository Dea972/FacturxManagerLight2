using FacturXManager.Common.Application.Messaging;

namespace FacturXManager.Modules.EReporting.Application.Declarations.SubmitDeclaration;

/// <summary>
/// Soumet une déclaration finalisée à la DGFiP et trace le statut de soumission.
/// </summary>
public sealed record SubmitDeclarationCommand(Guid DeclarationId) : ICommand<SubmitDeclarationSummary>;
