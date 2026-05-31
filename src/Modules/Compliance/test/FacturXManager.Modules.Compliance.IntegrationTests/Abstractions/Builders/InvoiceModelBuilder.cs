using FacturXManager.Modules.Compliance.Application.Abstractions;

namespace FacturXManager.Modules.Compliance.IntegrationTests.Abstractions.Builders;

/// <summary>
/// Builder fluent pour produire des <see cref="InvoiceModel"/> de test.
/// Valeurs par défaut : facture B2B française émise après le 1er sept. 2026, profil EN16931 Comfort.
/// </summary>
public sealed class InvoiceModelBuilder
{
    private string _number = "F-2026-0001";
    private DateOnly _issueDate = new(2026, 9, 15);
    private string _currencyCode = "EUR";
    private string _profile = "EN16931";
    private string _operationNature = "Services";
    private PartyModel _seller = new PartyModelBuilder().AsFrenchSeller().Build();
    private PartyModel _buyer = new PartyModelBuilder().AsFrenchBuyer().Build();
    private readonly PaymentModel _payment = new(PaymentMeansCode.SepaCreditTransfer, new DateOnly(2026, 10, 15), "FR7630006000011234567890189");
    private readonly List<InvoiceLineModel> _lines = [new InvoiceLineModelBuilder().Build()];
    private string? _buyerReference;
    private string? _purchaseOrderReference;

    public InvoiceModelBuilder WithNumber(string number) { _number = number; return this; }

    public InvoiceModelBuilder WithIssueDate(DateOnly date) { _issueDate = date; return this; }

    public InvoiceModelBuilder WithCurrency(string code) { _currencyCode = code; return this; }

    public InvoiceModelBuilder WithProfile(string profile) { _profile = profile; return this; }

    public InvoiceModelBuilder WithOperationNature(string nature) { _operationNature = nature; return this; }

    public InvoiceModelBuilder WithSeller(Func<PartyModelBuilder, PartyModelBuilder> configure)
    {
        _seller = configure(new PartyModelBuilder().AsFrenchSeller()).Build();
        return this;
    }

    public InvoiceModelBuilder WithBuyer(Func<PartyModelBuilder, PartyModelBuilder> configure)
    {
        _buyer = configure(new PartyModelBuilder().AsFrenchBuyer()).Build();
        return this;
    }

    public InvoiceModelBuilder WithoutLines() { _lines.Clear(); return this; }

    public InvoiceModelBuilder WithLine(Func<InvoiceLineModelBuilder, InvoiceLineModelBuilder> configure)
    {
        _lines.Add(configure(new InvoiceLineModelBuilder()).Build());
        return this;
    }

    public InvoiceModelBuilder ReplaceLines(params InvoiceLineModel[] lines)
    {
        _lines.Clear();
        _lines.AddRange(lines);
        return this;
    }

    public InvoiceModelBuilder WithBuyerReference(string reference) { _buyerReference = reference; return this; }

    public InvoiceModelBuilder WithPurchaseOrderReference(string reference)
    {
        _purchaseOrderReference = reference;
        return this;
    }

    public InvoiceModel Build() =>
        new(_number, _issueDate, _currencyCode, _profile, _operationNature, _seller, _buyer, _payment,
            _lines.ToList(), _buyerReference, _purchaseOrderReference);
}

public sealed class PartyModelBuilder
{
    private string _name = "Partie";
    private string _addressLine1 = "1 rue de Rivoli";
    private string _postalCode = "75001";
    private string _city = "Paris";
    private string _countryCode = "FR";
    private string? _legalRegistrationId;
    private string? _vatIdentifier;

    public PartyModelBuilder WithName(string name) { _name = name; return this; }

    public PartyModelBuilder WithSiren(string? siren) { _legalRegistrationId = siren; return this; }

    public PartyModelBuilder WithoutSiren() { _legalRegistrationId = null; return this; }

    public PartyModelBuilder WithVatIdentifier(string? vat) { _vatIdentifier = vat; return this; }

    public PartyModelBuilder WithCountry(string code) { _countryCode = code; return this; }

    public PartyModelBuilder AsFrenchSeller()
    {
        _name = "Vendeur SA";
        _legalRegistrationId = "552120222";
        _vatIdentifier = "FR00552120222";
        return this;
    }

    public PartyModelBuilder AsFrenchBuyer()
    {
        _name = "Acheteur SAS";
        _addressLine1 = "10 place Bellecour";
        _postalCode = "69001";
        _city = "Lyon";
        _legalRegistrationId = "732829320";
        return this;
    }

    public PartyModel Build() =>
        new(_name, _addressLine1, _postalCode, _city, _countryCode, null, _legalRegistrationId, _vatIdentifier);
}

public sealed class InvoiceLineModelBuilder
{
    private string _id = "1";
    private string _description = "Prestation de conseil";
    private decimal _quantity = 1m;
    private decimal _unitNetPrice = 100m;
    private string _vatCategory = "Standard";
    private decimal _vatRate = 20m;
    private readonly string _unitCode = "C62";

    public InvoiceLineModelBuilder WithId(string id) { _id = id; return this; }

    public InvoiceLineModelBuilder WithDescription(string description) { _description = description; return this; }

    public InvoiceLineModelBuilder WithQuantity(decimal quantity) { _quantity = quantity; return this; }

    public InvoiceLineModelBuilder WithUnitPrice(decimal price) { _unitNetPrice = price; return this; }

    public InvoiceLineModelBuilder WithVatCategory(string category) { _vatCategory = category; return this; }

    public InvoiceLineModelBuilder WithVatRate(decimal rate) { _vatRate = rate; return this; }

    public InvoiceLineModel Build() =>
        new(_id, _description, _quantity, _unitNetPrice, _vatCategory, _vatRate, _unitCode);
}
