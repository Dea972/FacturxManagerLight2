using FacturXManager.Common.Application.Messaging;

namespace FacturXManager.Modules.EReporting.Application.Declarations.FinalizeDeclaration;

/// <summary>
/// Finalise une déclaration : la fige avant soumission.
/// </summary>
public sealed record FinalizeDeclarationCommand(Guid DeclarationId) : ICommand;
