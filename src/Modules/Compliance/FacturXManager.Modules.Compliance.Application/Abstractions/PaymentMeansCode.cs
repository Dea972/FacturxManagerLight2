namespace FacturXManager.Modules.Compliance.Application.Abstractions;

/// <summary>
/// Codes de moyen de paiement acceptés par Chorus Pro (sous-ensemble UNCL4461).
/// Valeurs numériques conformes à la norme UN/CEFACT.
/// </summary>
public enum PaymentMeansCode
{
    /// <summary>Virement bancaire (30).</summary>
    BankTransfer = 30,

    /// <summary>Virement de débit (42).</summary>
    DebitTransfer = 42,

    /// <summary>Carte bancaire (48).</summary>
    BankCard = 48,

    /// <summary>Prélèvement bancaire (49).</summary>
    DirectDebit = 49,

    /// <summary>Virement SEPA (58).</summary>
    SepaCreditTransfer = 58,

    /// <summary>Prélèvement SEPA (59).</summary>
    SepaDirectDebit = 59,

    /// <summary>Compensation entre partenaires (97).</summary>
    ClearingBetweenPartners = 97,
}

public static class PaymentMeansCodeExtensions
{
    /// <summary>
    /// Retourne le code numérique UNCL4461 sous forme de chaîne (ex : BankTransfer → "30").
    /// </summary>
    public static string ToUnclCode(this PaymentMeansCode code) => code switch
    {
        PaymentMeansCode.BankTransfer => "30",
        PaymentMeansCode.DebitTransfer => "42",
        PaymentMeansCode.BankCard => "48",
        PaymentMeansCode.DirectDebit => "49",
        PaymentMeansCode.SepaCreditTransfer => "58",
        PaymentMeansCode.SepaDirectDebit => "59",
        PaymentMeansCode.ClearingBetweenPartners => "97",
        _ => "30",
    };
}
