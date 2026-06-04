namespace FacturXManager.Modules.Compliance.Domain.Common;

/// <summary>
/// Nature de l'opération facturée. Obligatoire à partir du 1er septembre 2026.
/// </summary>
public enum OperationNature
{
    Unspecified = 0,

    /// <summary>Livraison de biens.</summary>
    Goods = 1,

    /// <summary>Prestation de services.</summary>
    Services = 2,

    /// <summary>Opération mixte (biens et services).</summary>
    Mixed = 3
}
