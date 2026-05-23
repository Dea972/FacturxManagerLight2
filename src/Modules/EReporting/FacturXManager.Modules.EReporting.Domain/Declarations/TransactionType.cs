namespace FacturXManager.Modules.EReporting.Domain.Declarations;

/// <summary>
/// Type de transaction couvert par l'e-reporting (hors facturation structurée).
/// </summary>
public enum TransactionType
{
    /// <summary>Ventes à des particuliers (B2C).</summary>
    B2C = 0,

    /// <summary>Transactions internationales.</summary>
    International = 1
}
