using FacturXManager.Common.Domain;
using FacturXManager.Modules.Compliance.Domain.Common;

namespace FacturXManager.Modules.Compliance.Domain.Invoices;

/// <summary>
/// Agrégat facture. Construit en mémoire pour être certifié à la demande ; non persisté.
/// Calcule la ventilation de TVA et les totaux à partir de ses lignes.
/// </summary>
public sealed class Invoice : Entity
{
    private readonly List<InvoiceLine> _lines = [];
    private readonly List<VatBreakdown> _vatBreakdowns = [];

    private Invoice()
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

    public PaymentDetails? Payment { get; private set; } = null!;

    public string? BuyerReference { get; private set; }

    public string? PurchaseOrderReference { get; private set; }

    /// <summary>Statut courant du document dans son cycle de vie Compliance.</summary>
    public DocumentStatus Status { get; private set; } = DocumentStatus.Draft;

    /// <summary>Identifiant de dépôt retourné par la plateforme (ex : Chorus Pro), si déposée.</summary>
    public string? ChannelDepositId { get; private set; }

    public IReadOnlyList<InvoiceLine> Lines => _lines.AsReadOnly();

    public IReadOnlyList<VatBreakdown> VatBreakdowns => _vatBreakdowns.AsReadOnly();

    /// <summary>Total HT (BT-109).</summary>
    public decimal TotalWithoutVat => decimal.Round(_lines.Sum(l => l.NetAmount), 2, MidpointRounding.AwayFromZero);

    /// <summary>Total de TVA (BT-110).</summary>
    public decimal TotalVat => decimal.Round(_vatBreakdowns.Sum(v => v.TaxAmount), 2, MidpointRounding.AwayFromZero);

    /// <summary>Total TTC (BT-112).</summary>
    public decimal TotalWithVat => decimal.Round(TotalWithoutVat + TotalVat, 2, MidpointRounding.AwayFromZero);

    /// <summary>Net à payer (BT-115).</summary>
    public decimal AmountDue => TotalWithVat;

    public static Result<Invoice> Create(
        string number,
        DateOnly issueDate,
        string currencyCode,
        FacturXProfile profile,
        OperationNature operationNature,
        Party? seller,
        Party? buyer,
        PaymentDetails? payment,
        IReadOnlyCollection<InvoiceLine> lines,
        string? buyerReference = null,
        string? purchaseOrderReference = null)
    {
        if (string.IsNullOrWhiteSpace(number))
        {
            return Result.Failure<Invoice>(InvoiceErrors.NumberRequired);
        }

        if (string.IsNullOrWhiteSpace(currencyCode))
        {
            return Result.Failure<Invoice>(InvoiceErrors.CurrencyRequired);
        }

        if (seller is null)
        {
            return Result.Failure<Invoice>(InvoiceErrors.SellerRequired);
        }

        if (buyer is null)
        {
            return Result.Failure<Invoice>(InvoiceErrors.BuyerRequired);
        }

        if (lines is null || lines.Count == 0)
        {
            return Result.Failure<Invoice>(InvoiceErrors.NoLines);
        }

        var invoice = new Invoice
        {
            Id = Guid.CreateVersion7(),
            Number = number.Trim(),
            IssueDate = issueDate,
            CurrencyCode = currencyCode.Trim().ToUpperInvariant(),
            Profile = profile,
            OperationNature = operationNature,
            Seller = seller,
            Buyer = buyer,
            Payment = payment,
            BuyerReference = string.IsNullOrWhiteSpace(buyerReference) ? null : buyerReference.Trim(),
            PurchaseOrderReference =
                string.IsNullOrWhiteSpace(purchaseOrderReference) ? null : purchaseOrderReference.Trim(),
        };

        invoice._lines.AddRange(lines);
        invoice.RecalculateVatBreakdowns();

        return invoice;
    }

    /// <summary>
    /// Indique si la facture relève de l'obligation B2B renforcée (à compter du 1er septembre 2026).
    /// </summary>
    public bool IsSubjectToB2BObligation() => IssueDate >= FacturXConstants.B2BObligationStartDate;

    /// <summary>Marque la facture comme certifiée (XML + PDF générés).</summary>
    public void MarkCertified() => Status = DocumentStatus.Certified;

    /// <summary>Marque la facture comme déposée avec succès sur une plateforme.</summary>
    public void MarkSubmitted(string? channelDepositId)
    {
        Status = DocumentStatus.Submitted;
        ChannelDepositId = string.IsNullOrWhiteSpace(channelDepositId) ? null : channelDepositId.Trim();
    }

    /// <summary>Marque l'échec du dépôt (rejet plateforme ou échec technique).</summary>
    public void MarkSubmissionFailed() => Status = DocumentStatus.SubmissionFailed;

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
