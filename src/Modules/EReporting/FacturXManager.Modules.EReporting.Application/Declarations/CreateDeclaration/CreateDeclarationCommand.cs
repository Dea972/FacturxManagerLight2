using FacturXManager.Common.Application.Messaging;

namespace FacturXManager.Modules.EReporting.Application.Declarations.CreateDeclaration;

/// <summary>
/// Crée une déclaration d'e-reporting au statut Brouillon pour une période et un type de transaction.
/// </summary>
public sealed record CreateDeclarationCommand(int Year, int Month, string TransactionType) : ICommand<Guid>;
