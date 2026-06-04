using FacturXManager.Common.Domain;
using FacturXManager.Modules.Compliance.Domain.CreditNotes;

namespace FacturXManager.Modules.Compliance.Domain.Common;

/// <summary>
/// Référence à la facture d'origine d'un avoir (BT-25 / BT-26 EN16931).
/// </summary>
public sealed class ReferencedInvoice : ValueObject
{
    private ReferencedInvoice(string number, DateOnly? issueDate)
    {
        Number = number;
        IssueDate = issueDate;
    }

    /// <summary>Numéro de la facture d'origine (BT-25, obligatoire).</summary>
    public string Number { get; }

    /// <summary>Date d'émission de la facture d'origine (BT-26, optionnel).</summary>
    public DateOnly? IssueDate { get; }

    public static Result<ReferencedInvoice> Create(string? number, DateOnly? issueDate = null)
    {
        if (string.IsNullOrWhiteSpace(number))
        {
            return Result.Failure<ReferencedInvoice>(CreditNoteErrors.OriginalInvoiceNumberRequired);
        }

        return new ReferencedInvoice(number.Trim(), issueDate);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Number;
        yield return IssueDate;
    }
}
