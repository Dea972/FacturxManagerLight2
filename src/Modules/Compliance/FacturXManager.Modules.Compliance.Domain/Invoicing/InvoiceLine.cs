using FacturXManager.Common.Domain;

namespace FacturXManager.Modules.Compliance.Domain.Invoicing;

/// <summary>
/// Ligne de facture (BG-25 EN16931).
/// </summary>
public sealed class InvoiceLine : ValueObject
{
    private InvoiceLine(
        string id,
        string description,
        decimal quantity,
        string unitCode,
        decimal unitNetPrice,
        VatCategory vatCategory,
        decimal vatRate)
    {
        Id = id;
        Description = description;
        Quantity = quantity;
        UnitCode = unitCode;
        UnitNetPrice = unitNetPrice;
        VatCategory = vatCategory;
        VatRate = vatRate;
    }

    public string Id { get; }

    public string Description { get; }

    public decimal Quantity { get; }

    /// <summary>Unité de mesure (code UN/ECE Rec 20, ex : C62 = unité).</summary>
    public string UnitCode { get; }

    public decimal UnitNetPrice { get; }

    public VatCategory VatCategory { get; }

    /// <summary>Taux de TVA en pourcentage (ex : 20 pour 20 %).</summary>
    public decimal VatRate { get; }

    /// <summary>Montant net HT de la ligne (BT-131).</summary>
    public decimal NetAmount => decimal.Round(Quantity * UnitNetPrice, 2, MidpointRounding.AwayFromZero);

    public static InvoiceLine Create(
        string id,
        string description,
        decimal quantity,
        decimal unitNetPrice,
        VatCategory vatCategory,
        decimal vatRate,
        string unitCode = "C62") =>
        new(
            id?.Trim() ?? string.Empty,
            description?.Trim() ?? string.Empty,
            quantity,
            string.IsNullOrWhiteSpace(unitCode) ? "C62" : unitCode.Trim(),
            unitNetPrice,
            vatCategory,
            vatRate);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Id;
        yield return Description;
        yield return Quantity;
        yield return UnitCode;
        yield return UnitNetPrice;
        yield return VatCategory;
        yield return VatRate;
    }
}
