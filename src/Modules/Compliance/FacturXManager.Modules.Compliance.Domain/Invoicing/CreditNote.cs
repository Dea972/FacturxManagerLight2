using FacturXManager.Common.Domain;

namespace FacturXManager.Modules.Compliance.Domain.Invoicing;

/// <summary>
/// Agrégat avoir Factur-X (type de document CII 381).
/// Construit en mémoire pour être certifié à la demande ; non persisté.
/// Calcule la ventilation de TVA et les totaux à partir de ses lignes.
/// </summary>
public sealed partial class CreditNote : Entity
{
    private readonly List<CreditNoteLine> _lines = [];
    private readonly List<VatBreakdown> _vatBreakdowns = [];

    private CreditNote()
    {
    }

    public Guid Id { get; private set; }

    public string Number { get; private set; } = string.Empty;

    public DateOnly IssueDate { get; private set; }

    /// <summary>Code devise ISO 4217 (ex : EUR).</summary>
    public string CurrencyCode { get; private set; } = string.Empty;

    public FacturXProfile Profile { get; private set; }

    public OperationNature OperationNature { get; private set; }

    public Party Seller { get; private set; } = null!;

    public Party Buyer { get; private set; } = null!;

    /// <summary>Référence à la facture d'origine (BT-25 / BT-26).</summary>
    public ReferencedInvoice OriginalInvoice { get; private set; } = null!;

    /// <summary>Motif de l'avoir.</summary>
    public string? CreditReason { get; private set; }

    public string? BuyerReference { get; private set; }

    /// <summary>Code moyen de paiement UNCL4461 (ex : 30 = virement). Obligatoire pour Chorus Pro.</summary>
    public string PaymentMeansCode { get; private set; } = "30";

    public IReadOnlyList<CreditNoteLine> Lines => _lines.AsReadOnly();

    public IReadOnlyList<VatBreakdown> VatBreakdowns => _vatBreakdowns.AsReadOnly();

    /// <summary>Total HT (BT-109).</summary>
    public decimal TotalWithoutVat => decimal.Round(_lines.Sum(l => l.NetAmount), 2, MidpointRounding.AwayFromZero);

    /// <summary>Total de TVA (BT-110).</summary>
    public decimal TotalVat => decimal.Round(_vatBreakdowns.Sum(v => v.TaxAmount), 2, MidpointRounding.AwayFromZero);

    /// <summary>Total TTC (BT-112).</summary>
    public decimal TotalWithVat => decimal.Round(TotalWithoutVat + TotalVat, 2, MidpointRounding.AwayFromZero);

    /// <summary>Montant dû (BT-115).</summary>
    public decimal AmountDue => TotalWithVat;

    public static Result<CreditNote> Create(
        string number,
        DateOnly issueDate,
        string currencyCode,
        FacturXProfile profile,
        OperationNature operationNature,
        Party? seller,
        Party? buyer,
        ReferencedInvoice? originalInvoice,
        IReadOnlyCollection<CreditNoteLine> lines,
        string? creditReason = null,
        string? buyerReference = null,
        string paymentMeansCode = "30")
    {
        if (string.IsNullOrWhiteSpace(number))
        {
            return Result.Failure<CreditNote>(CreditNoteErrors.NumberRequired);
        }

        if (string.IsNullOrWhiteSpace(currencyCode))
        {
            return Result.Failure<CreditNote>(CreditNoteErrors.CurrencyRequired);
        }

        if (seller is null)
        {
            return Result.Failure<CreditNote>(CreditNoteErrors.SellerRequired);
        }

        if (buyer is null)
        {
            return Result.Failure<CreditNote>(CreditNoteErrors.BuyerRequired);
        }

        if (originalInvoice is null)
        {
            return Result.Failure<CreditNote>(CreditNoteErrors.OriginalInvoiceRequired);
        }

        if (lines is null || lines.Count == 0)
        {
            return Result.Failure<CreditNote>(CreditNoteErrors.NoLines);
        }

        var creditNote = new CreditNote
        {
            Id = Guid.NewGuid(),
            Number = number.Trim(),
            IssueDate = issueDate,
            CurrencyCode = currencyCode.Trim().ToUpperInvariant(),
            Profile = profile,
            OperationNature = operationNature,
            Seller = seller,
            Buyer = buyer,
            OriginalInvoice = originalInvoice,
            CreditReason = string.IsNullOrWhiteSpace(creditReason) ? null : creditReason.Trim(),
            BuyerReference = string.IsNullOrWhiteSpace(buyerReference) ? null : buyerReference.Trim(),
            PaymentMeansCode = string.IsNullOrWhiteSpace(paymentMeansCode) ? "30" : paymentMeansCode.Trim(),
        };

        creditNote._lines.AddRange(lines);
        creditNote.RecalculateVatBreakdowns();

        return creditNote;
    }

    private void RecalculateVatBreakdowns()
    {
        _vatBreakdowns.Clear();

        IEnumerable<VatBreakdown> breakdowns = _lines
            .GroupBy(l => new { l.VatCategory, l.VatRate })
            .Select(g => VatBreakdown.Create(g.Key.VatCategory, g.Key.VatRate, g.Sum(l => l.NetAmount)))
            .OrderBy(b => b.Rate);

        _vatBreakdowns.AddRange(breakdowns);
    }
}
