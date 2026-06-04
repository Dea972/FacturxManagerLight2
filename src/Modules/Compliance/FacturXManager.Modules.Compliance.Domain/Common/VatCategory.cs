namespace FacturXManager.Modules.Compliance.Domain.Common;

/// <summary>
/// Catégories de TVA EN16931 (code UNCL5305).
/// </summary>
public enum VatCategory
{
    /// <summary>Taux standard (S).</summary>
    Standard = 0,

    /// <summary>Taux zéro (Z).</summary>
    ZeroRated = 1,

    /// <summary>Exonéré de TVA (E).</summary>
    Exempt = 2,

    /// <summary>Autoliquidation (AE).</summary>
    ReverseCharge = 3,

    /// <summary>Livraison intracommunautaire exonérée (K).</summary>
    IntraCommunity = 4,

    /// <summary>Export hors Union européenne (G).</summary>
    Export = 5,

    /// <summary>Hors champ de la TVA (O).</summary>
    OutOfScope = 6
}

public static class VatCategoryExtensions
{
    /// <summary>Retourne le code UNCL5305 attendu dans le XML CII.</summary>
    public static string ToUnclCode(this VatCategory category) =>
        category switch
        {
            VatCategory.Standard => "S",
            VatCategory.ZeroRated => "Z",
            VatCategory.Exempt => "E",
            VatCategory.ReverseCharge => "AE",
            VatCategory.IntraCommunity => "K",
            VatCategory.Export => "G",
            VatCategory.OutOfScope => "O",
            _ => "S"
        };

    /// <summary>Indique si la catégorie autorise un taux strictement positif.</summary>
    public static bool AllowsPositiveRate(this VatCategory category) =>
        category is VatCategory.Standard;
}
