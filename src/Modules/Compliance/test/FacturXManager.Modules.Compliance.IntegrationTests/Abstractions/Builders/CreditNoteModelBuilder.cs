using FacturXManager.Modules.Compliance.Application.Abstractions;

namespace FacturXManager.Modules.Compliance.IntegrationTests.Abstractions.Builders;

/// <summary>
/// Builder fluent pour produire des <see cref="CreditNoteModel"/> de test.
/// Valeurs par défaut : avoir B2B français émis après le 1er sept. 2026, profil EN16931 Comfort.
/// </summary>
public sealed class CreditNoteModelBuilder
{
    private string _number = "AV-2026-0001";
    private DateOnly _issueDate = new(2026, 9, 20);
    private string _currencyCode = "EUR";
    private string _profile = "EN16931";
    private string _operationNature = "Services";
    private PartyModel _seller = new PartyModelBuilder().AsFrenchSeller().Build();
    private PartyModel _buyer = new PartyModelBuilder().AsFrenchBuyer().Build();
    private string _originalInvoiceNumber = "F-2026-0001";
    private DateOnly? _originalInvoiceIssueDate = new DateOnly(2026, 9, 15);
    private string? _creditReason = "Erreur de facturation";
    private string? _buyerReference;
    private readonly List<CreditNoteLineModel> _lines = [new CreditNoteLineModelBuilder().Build()];

    public CreditNoteModelBuilder WithNumber(string number) { _number = number; return this; }

    public CreditNoteModelBuilder WithIssueDate(DateOnly date) { _issueDate = date; return this; }

    public CreditNoteModelBuilder WithCurrency(string code) { _currencyCode = code; return this; }

    public CreditNoteModelBuilder WithProfile(string profile) { _profile = profile; return this; }

    public CreditNoteModelBuilder WithOperationNature(string nature) { _operationNature = nature; return this; }

    public CreditNoteModelBuilder WithSeller(Func<PartyModelBuilder, PartyModelBuilder> configure)
    {
        _seller = configure(new PartyModelBuilder().AsFrenchSeller()).Build();
        return this;
    }

    public CreditNoteModelBuilder WithBuyer(Func<PartyModelBuilder, PartyModelBuilder> configure)
    {
        _buyer = configure(new PartyModelBuilder().AsFrenchBuyer()).Build();
        return this;
    }

    public CreditNoteModelBuilder WithOriginalInvoice(string number, DateOnly? issueDate = null)
    {
        _originalInvoiceNumber = number;
        _originalInvoiceIssueDate = issueDate;
        return this;
    }

    public CreditNoteModelBuilder WithCreditReason(string? reason) { _creditReason = reason; return this; }

    public CreditNoteModelBuilder WithBuyerReference(string reference) { _buyerReference = reference; return this; }

    public CreditNoteModelBuilder WithoutLines() { _lines.Clear(); return this; }

    public CreditNoteModelBuilder WithLine(Func<CreditNoteLineModelBuilder, CreditNoteLineModelBuilder> configure)
    {
        _lines.Add(configure(new CreditNoteLineModelBuilder()).Build());
        return this;
    }

    public CreditNoteModel Build() =>
        new(_number, _issueDate, _currencyCode, _profile, _operationNature, _seller, _buyer,
            _originalInvoiceNumber, _lines.ToList(), _originalInvoiceIssueDate, _creditReason, _buyerReference);
}

public sealed class CreditNoteLineModelBuilder
{
    private string _id = "1";
    private string _description = "Annulation prestation de conseil";
    private decimal _quantity = 1m;
    private decimal _unitNetPrice = 100m;
    private string _vatCategory = "Standard";
    private decimal _vatRate = 20m;
    private readonly string _unitCode = "C62";

    public CreditNoteLineModelBuilder WithId(string id) { _id = id; return this; }

    public CreditNoteLineModelBuilder WithDescription(string description) { _description = description; return this; }

    public CreditNoteLineModelBuilder WithQuantity(decimal quantity) { _quantity = quantity; return this; }

    public CreditNoteLineModelBuilder WithUnitPrice(decimal price) { _unitNetPrice = price; return this; }

    public CreditNoteLineModelBuilder WithVatCategory(string category) { _vatCategory = category; return this; }

    public CreditNoteLineModelBuilder WithVatRate(decimal rate) { _vatRate = rate; return this; }

    public CreditNoteLineModel Build() =>
        new(_id, _description, _quantity, _unitNetPrice, _vatCategory, _vatRate, _unitCode);
}
