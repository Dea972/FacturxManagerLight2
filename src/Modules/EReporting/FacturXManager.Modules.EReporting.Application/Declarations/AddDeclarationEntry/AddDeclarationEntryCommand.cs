using FacturXManager.Common.Application.Messaging;

namespace FacturXManager.Modules.EReporting.Application.Declarations.AddDeclarationEntry;

/// <summary>
/// Ajoute une transaction (montant HT, TVA, pays, nature) à une déclaration au statut Brouillon.
/// </summary>
public sealed record AddDeclarationEntryCommand(
    Guid DeclarationId,
    decimal TaxableAmount,
    decimal VatAmount,
    string CountryCode,
    string OperationNature) : ICommand;
