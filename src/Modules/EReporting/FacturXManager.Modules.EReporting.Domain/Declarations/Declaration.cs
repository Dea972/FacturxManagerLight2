using FacturXManager.Common.Domain;

namespace FacturXManager.Modules.EReporting.Domain.Declarations;

/// <summary>
/// Agrégat de déclaration d'e-reporting : couvre une période et un type de transaction,
/// agrège des transactions et trace le cycle de soumission DGFiP.
/// </summary>
public sealed class Declaration : Entity
{
    private readonly List<DeclarationEntry> _entries = [];

    private Declaration()
    {
    }

    public Guid Id { get; private set; }

    public ReportingPeriod Period { get; private set; } = null!;

    public TransactionType TransactionType { get; private set; }

    public DeclarationStatus Status { get; private set; }

    public string? DgfipSubmissionReference { get; private set; }

    public DateTime CreatedOnUtc { get; private set; }

    public DateTime? SubmittedOnUtc { get; private set; }

    public DateTime? AcceptedOnUtc { get; private set; }

    public IReadOnlyList<DeclarationEntry> Entries => _entries.AsReadOnly();

    public decimal TotalTaxableAmount =>
        decimal.Round(_entries.Sum(e => e.TaxableAmount), 2, MidpointRounding.AwayFromZero);

    public decimal TotalVatAmount =>
        decimal.Round(_entries.Sum(e => e.VatAmount), 2, MidpointRounding.AwayFromZero);

    public static Declaration Create(ReportingPeriod period, TransactionType transactionType, DateTime createdOnUtc) =>
        new()
        {
            Id = Guid.NewGuid(),
            Period = period,
            TransactionType = transactionType,
            Status = DeclarationStatus.Draft,
            CreatedOnUtc = createdOnUtc,
        };

    public Result AddEntry(decimal taxableAmount, decimal vatAmount, string countryCode, OperationNature nature)
    {
        if (Status != DeclarationStatus.Draft)
        {
            return Result.Failure(DeclarationErrors.NotEditable);
        }

        if (taxableAmount <= 0)
        {
            return Result.Failure(DeclarationErrors.EntryAmountInvalid);
        }

        if (string.IsNullOrWhiteSpace(countryCode))
        {
            return Result.Failure(DeclarationErrors.CountryRequired);
        }

        _entries.Add(DeclarationEntry.Create(Id, taxableAmount, vatAmount, countryCode, nature));

        return Result.Success();
    }

    public Result Finalize()
    {
        if (Status != DeclarationStatus.Draft)
        {
            return Result.Failure(DeclarationErrors.NotEditable);
        }

        if (_entries.Count == 0)
        {
            return Result.Failure(DeclarationErrors.CannotFinalizeEmpty);
        }

        Status = DeclarationStatus.Finalized;

        return Result.Success();
    }

    public Result MarkSubmitted(string dgfipSubmissionReference, DateTime occurredOnUtc)
    {
        if (Status != DeclarationStatus.Finalized)
        {
            return Result.Failure(DeclarationErrors.NotFinalized);
        }

        Status = DeclarationStatus.Submitted;
        DgfipSubmissionReference = dgfipSubmissionReference;
        SubmittedOnUtc = occurredOnUtc;

        return Result.Success();
    }

    public Result MarkAccepted(DateTime occurredOnUtc)
    {
        if (Status != DeclarationStatus.Submitted)
        {
            return Result.Failure(DeclarationErrors.NotSubmitted);
        }

        Status = DeclarationStatus.Accepted;
        AcceptedOnUtc = occurredOnUtc;

        return Result.Success();
    }
}
