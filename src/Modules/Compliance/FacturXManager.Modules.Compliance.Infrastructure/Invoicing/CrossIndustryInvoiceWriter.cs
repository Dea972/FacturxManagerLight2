using System.Globalization;
using System.Xml.Linq;
using FacturXManager.Modules.Compliance.Application.Abstractions;
using FacturXManager.Modules.Compliance.Domain.Invoicing;

// S1192 : les noms d'éléments XML CII (ID, Name, TypeCode, etc.) sont répétés par nature.
#pragma warning disable S1192
namespace FacturXManager.Modules.Compliance.Infrastructure.Invoicing;

/// <summary>
/// Génère le XML Cross Industry Invoice (UN/CEFACT D16B) conforme EN16931.
/// Implémentation sans dépendance externe (System.Xml.Linq).
/// </summary>
internal sealed class CrossIndustryInvoiceWriter : ICrossIndustryInvoiceWriter
{
    private static readonly XNamespace Rsm =
        "urn:un:unece:uncefact:data:standard:CrossIndustryInvoice:100";

    private static readonly XNamespace Ram =
        "urn:un:unece:uncefact:data:standard:ReusableAggregateBusinessInformationEntity:100";

    private static readonly XNamespace Udt =
        "urn:un:unece:uncefact:data:standard:UnqualifiedDataType:100";

    public string Write(Invoice invoice)
    {
        var document = new XDocument(
            new XDeclaration("1.0", "UTF-8", null),
            new XElement(
                Rsm + "CrossIndustryInvoice",
                new XAttribute(XNamespace.Xmlns + "rsm", Rsm.NamespaceName),
                new XAttribute(XNamespace.Xmlns + "ram", Ram.NamespaceName),
                new XAttribute(XNamespace.Xmlns + "udt", Udt.NamespaceName),
                BuildDocumentContext(invoice),
                BuildExchangedDocument(invoice),
                BuildTradeTransaction(invoice)));

        string body = document.ToString();

        return document.Declaration is null
            ? body
            : $"{document.Declaration}{Environment.NewLine}{body}";
    }

    private static XElement BuildDocumentContext(Invoice invoice)
    {
        string guidelineId = invoice.Profile == FacturXProfile.Extended
            ? FacturXConstants.ExtendedGuidelineId
            : FacturXConstants.En16931GuidelineId;

        return new XElement(
            Rsm + "ExchangedDocumentContext",
            new XElement(
                Ram + "GuidelineSpecifiedDocumentContextParameter",
                new XElement(Ram + "ID", guidelineId)));
    }

    private static XElement BuildExchangedDocument(Invoice invoice) =>
        new(
            Rsm + "ExchangedDocument",
            new XElement(Ram + "ID", invoice.Number),
            new XElement(Ram + "TypeCode", "380"),
            new XElement(
                Ram + "IssueDateTime",
                new XElement(
                    Udt + "DateTimeString",
                    new XAttribute("format", "102"),
                    invoice.IssueDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture))));

    private static XElement BuildTradeTransaction(Invoice invoice)
    {
        var transaction = new XElement(Rsm + "SupplyChainTradeTransaction");

        foreach (InvoiceLine line in invoice.Lines)
        {
            transaction.Add(BuildLineItem(line));
        }

        transaction.Add(BuildHeaderTradeAgreement(invoice));
        transaction.Add(new XElement(Ram + "ApplicableHeaderTradeDelivery"));
        transaction.Add(BuildHeaderTradeSettlement(invoice));

        return transaction;
    }

    private static XElement BuildLineItem(InvoiceLine line) =>
        new(
            Ram + "IncludedSupplyChainTradeLineItem",
            new XElement(
                Ram + "AssociatedDocumentLineDocument",
                new XElement(Ram + "LineID", line.Id)),
            new XElement(
                Ram + "SpecifiedTradeProduct",
                new XElement(Ram + "Name", line.Description)),
            new XElement(
                Ram + "SpecifiedLineTradeAgreement",
                new XElement(
                    Ram + "NetPriceProductTradePrice",
                    new XElement(Ram + "ChargeAmount", Amount(line.UnitNetPrice)))),
            new XElement(
                Ram + "SpecifiedLineTradeDelivery",
                new XElement(
                    Ram + "BilledQuantity",
                    new XAttribute("unitCode", line.UnitCode),
                    Quantity(line.Quantity))),
            new XElement(
                Ram + "SpecifiedLineTradeSettlement",
                new XElement(
                    Ram + "ApplicableTradeTax",
                    new XElement(Ram + "TypeCode", "VAT"),
                    new XElement(Ram + "CategoryCode", line.VatCategory.ToUnclCode()),
                    new XElement(Ram + "RateApplicablePercent", Rate(line.VatRate))),
                new XElement(
                    Ram + "SpecifiedTradeSettlementLineMonetarySummation",
                    new XElement(Ram + "LineTotalAmount", Amount(line.NetAmount)))));

    private static XElement BuildHeaderTradeAgreement(Invoice invoice)
    {
        var agreement = new XElement(Ram + "ApplicableHeaderTradeAgreement");

        if (!string.IsNullOrWhiteSpace(invoice.BuyerReference))
        {
            agreement.Add(new XElement(Ram + "BuyerReference", invoice.BuyerReference));
        }

        agreement.Add(BuildTradeParty(Ram + "SellerTradeParty", invoice.Seller, includeVat: true));
        agreement.Add(BuildTradeParty(Ram + "BuyerTradeParty", invoice.Buyer, includeVat: true));

        if (!string.IsNullOrWhiteSpace(invoice.PurchaseOrderReference))
        {
            agreement.Add(new XElement(
                Ram + "BuyerOrderReferencedDocument",
                new XElement(Ram + "IssuerAssignedID", invoice.PurchaseOrderReference)));
        }

        return agreement;
    }

    private static XElement BuildTradeParty(XName elementName, Party party, bool includeVat)
    {
        var element = new XElement(elementName, new XElement(Ram + "Name", party.Name));

        if (party.HasLegalRegistrationId)
        {
            element.Add(new XElement(
                Ram + "SpecifiedLegalOrganization",
                new XElement(Ram + "ID",
                    new XAttribute("schemeID", "0002"),
                    party.LegalRegistrationId)));
        }

        if (party.HasLegalOrganizationId)
        {
            element.Add(new XElement(
                Ram + "SpecifiedLegalOrganization",
                new XElement(Ram + "ID",
                    new XAttribute("schemeID", "0088"),
                    party.LegalOrganizationId)));
        }

        element.Add(new XElement(
            Ram + "PostalTradeAddress",
            new XElement(Ram + "PostcodeCode", party.Address.PostalCode),
            new XElement(Ram + "LineOne", party.Address.Line1),
            party.Address.Line2 is null ? null : new XElement(Ram + "LineTwo", party.Address.Line2),
            new XElement(Ram + "CityName", party.Address.City),
            new XElement(Ram + "CountryID", party.Address.CountryCode)));

        if (includeVat && !string.IsNullOrWhiteSpace(party.VatIdentifier))
        {
            element.Add(new XElement(
                Ram + "SpecifiedTaxRegistration",
                new XElement(
                    Ram + "ID",
                    new XAttribute("schemeID", "VA"),
                    party.VatIdentifier)));
        }

        if (!string.IsNullOrWhiteSpace(party.ElectronicAddress))
        {
            element.Add(new XElement(
                Ram + "URIUniversalCommunication",
                new XElement(Ram + "URIID",
                    new XAttribute("schemeID", party.ElectronicAddressScheme ?? "EM"),
                    party.ElectronicAddress)));
        }

        return element;
    }

    private static XElement BuildHeaderTradeSettlement(Invoice invoice)
    {
        var settlement = new XElement(
            Ram + "ApplicableHeaderTradeSettlement",
            new XElement(Ram + "InvoiceCurrencyCode", invoice.CurrencyCode));

        var paymentMeans = new XElement(
            Ram + "SpecifiedTradeSettlementPaymentMeans",
            new XElement(Ram + "TypeCode", invoice.Payment.MeansCode));

        if (!string.IsNullOrWhiteSpace(invoice.Payment.Iban))
        {
            paymentMeans.Add(new XElement(
                Ram + "PayeePartyCreditorFinancialAccount",
                new XElement(Ram + "IBANID", invoice.Payment.Iban)));
        }

        settlement.Add(paymentMeans);

        foreach (VatBreakdown breakdown in invoice.VatBreakdowns)
        {
            settlement.Add(new XElement(
                Ram + "ApplicableTradeTax",
                new XElement(Ram + "CalculatedAmount", Amount(breakdown.TaxAmount)),
                new XElement(Ram + "TypeCode", "VAT"),
                new XElement(Ram + "BasisAmount", Amount(breakdown.BasisAmount)),
                new XElement(Ram + "CategoryCode", breakdown.Category.ToUnclCode()),
                new XElement(Ram + "RateApplicablePercent", Rate(breakdown.Rate))));
        }

        if (invoice.Payment.DueDate is { } dueDate)
        {
            settlement.Add(new XElement(
                Ram + "SpecifiedTradePaymentTerms",
                new XElement(
                    Ram + "DueDateDateTime",
                    new XElement(
                        Udt + "DateTimeString",
                        new XAttribute("format", "102"),
                        dueDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture)))));
        }

        settlement.Add(new XElement(
            Ram + "SpecifiedTradeSettlementHeaderMonetarySummation",
            new XElement(Ram + "LineTotalAmount", Amount(invoice.TotalWithoutVat)),
            new XElement(Ram + "TaxBasisTotalAmount", Amount(invoice.TotalWithoutVat)),
            new XElement(
                Ram + "TaxTotalAmount",
                new XAttribute("currencyID", invoice.CurrencyCode),
                Amount(invoice.TotalVat)),
            new XElement(Ram + "GrandTotalAmount", Amount(invoice.TotalWithVat)),
            new XElement(Ram + "DuePayableAmount", Amount(invoice.AmountDue))));

        return settlement;
    }

    private static string Amount(decimal value) => value.ToString("0.00", CultureInfo.InvariantCulture);

    private static string Rate(decimal value) => value.ToString("0.00", CultureInfo.InvariantCulture);

    private static string Quantity(decimal value) => value.ToString("0.####", CultureInfo.InvariantCulture);
}
