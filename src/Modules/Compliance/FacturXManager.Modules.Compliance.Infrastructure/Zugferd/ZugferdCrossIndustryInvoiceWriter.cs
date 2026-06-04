using s2industries.ZUGFeRD;
using FacturXManager.Modules.Compliance.Application.Abstractions;
using System.Globalization;
using FacturXManager.Modules.Compliance.Domain.CreditNotes;
using FacturXManager.Modules.Compliance.Domain.Invoices;
using FacturXManager.Modules.Compliance.Domain.Common;

namespace FacturXManager.Modules.Compliance.Infrastructure.Zugferd;

/// <summary>
/// Génère le XML Cross Industry Invoice (CII) conforme EN16931 via la bibliothèque ZUGFeRD-csharp.
/// Implémentation alternative à <see cref="PdfSharp.CrossIndustryInvoiceWriter"/> (System.Xml.Linq).
/// </summary>
internal sealed class ZugferdCrossIndustryInvoiceWriter : ICrossIndustryInvoiceWriter
{
    public string Write(Invoice invoice)
    {
        var descriptor = InvoiceDescriptor.CreateInvoice(
            invoice.Number,
            invoice.IssueDate.ToDateTime(TimeOnly.MinValue),
            MapCurrencyCode(invoice.CurrencyCode));

        descriptor.Type = InvoiceType.Invoice;
        descriptor.ReferenceOrderNo = invoice.PurchaseOrderReference ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(invoice.BuyerReference))
        {
            descriptor.SetBuyerOrderReferenceDocument(invoice.BuyerReference);
        }

        // Vendeur (BG-4)
        descriptor.SetSeller(
            invoice.Seller.Name,
            invoice.Seller.Address.PostalCode,
            invoice.Seller.Address.City,
            invoice.Seller.Address.Line1,
            MapCountryCode(invoice.Seller.Address.CountryCode),
            legalOrganization: invoice.Seller.HasLegalOrganizationId ? new LegalOrganization(GlobalIDSchemeIdentifiers.Sirene, invoice.Seller.LegalOrganizationId) : null);

        if (!string.IsNullOrWhiteSpace(invoice.Seller.VatIdentifier))
        {
            descriptor.AddSellerTaxRegistration(invoice.Seller.VatIdentifier, TaxRegistrationSchemeID.VA);
        }

        if (!string.IsNullOrWhiteSpace(invoice.Seller.LegalRegistrationId))
        {
            descriptor.AddSellerTaxRegistration(invoice.Seller.LegalRegistrationId, TaxRegistrationSchemeID.FC);
        }

        if (!string.IsNullOrWhiteSpace(invoice.Seller.ElectronicAddress))
        {
            descriptor.SetSellerElectronicAddress(
                invoice.Seller.ElectronicAddress,
                MapElectronicAddressScheme(invoice.Seller.ElectronicAddressScheme));
        }

        // Acheteur (BG-7)
        descriptor.SetBuyer(
            invoice.Buyer.Name,
            invoice.Buyer.Address.PostalCode,
            invoice.Buyer.Address.City,
            invoice.Buyer.Address.Line1,
            MapCountryCode(invoice.Buyer.Address.CountryCode),
            legalOrganization: invoice.Buyer.HasLegalOrganizationId ? new LegalOrganization(GlobalIDSchemeIdentifiers.Sirene, invoice.Buyer.LegalOrganizationId) : null);

        if (!string.IsNullOrWhiteSpace(invoice.Buyer.VatIdentifier))
        {
            descriptor.AddBuyerTaxRegistration(invoice.Buyer.VatIdentifier, TaxRegistrationSchemeID.VA);
        }

        if (!string.IsNullOrWhiteSpace(invoice.Buyer.LegalRegistrationId))
        {
            descriptor.AddBuyerTaxRegistration(invoice.Buyer.LegalRegistrationId, TaxRegistrationSchemeID.FC);
            
        }


        if (!string.IsNullOrWhiteSpace(invoice.Buyer.ElectronicAddress))
        {
            descriptor.SetBuyerElectronicAddress(
                invoice.Buyer.ElectronicAddress,
                MapElectronicAddressScheme(invoice.Buyer.ElectronicAddressScheme));
        }

        // Livraison (BG-13) — obligatoire dans le XSD CII même si vide
        descriptor.ActualDeliveryDate = invoice.IssueDate.ToDateTime(TimeOnly.MinValue);

        // Modalités de paiement (BG-16)
        descriptor.SetPaymentMeans(
            MapPaymentMeansCode(invoice.Payment.MeansCode),
            string.Empty);

        if (invoice.Payment.DueDate.HasValue)
        {
            descriptor.SetPaymentMeans(
                MapPaymentMeansCode(invoice.Payment.MeansCode),
                string.Empty);
            descriptor.AddTradePaymentTerms(
                string.Empty,
                invoice.Payment.DueDate.Value.ToDateTime(TimeOnly.MinValue));
        }

        if (!string.IsNullOrWhiteSpace(invoice.Payment.Iban))
        {
            descriptor.SetPaymentMeansSepaDirectDebit(
                string.Empty,
                invoice.Payment.Iban);
        }

        // Lignes de facture (BG-25)
        foreach (InvoiceLine line in invoice.Lines)
        {
            int index = invoice.Lines.ToList().IndexOf(line) + 1;

            descriptor.AddTradeLineItem(
                lineID: index.ToString(CultureInfo.InvariantCulture),
                name: line.Description,
                billedQuantity: line.Quantity,
                unitCode: MapQuantityCode(line.UnitCode),
                netUnitPrice: line.UnitNetPrice,
                categoryCode: MapTaxCategoryCode(line.VatCategory),
                taxPercent: line.VatRate,
                taxType: TaxTypes.VAT);
        }

        // Ventilation TVA (BG-23)
        foreach (VatBreakdown breakdown in invoice.VatBreakdowns)
        {
            descriptor.AddApplicableTradeTax(
                breakdown.BasisAmount,
                breakdown.Rate,
                breakdown.TaxAmount,
                TaxTypes.VAT,
                MapTaxCategoryCode(breakdown.Category));
        }

        // Totaux
        descriptor.SetTotals(
            lineTotalAmount: invoice.TotalWithoutVat,
            taxBasisAmount: invoice.TotalWithoutVat,
            taxTotalAmount: invoice.TotalVat,
            grandTotalAmount: invoice.TotalWithVat,
            duePayableAmount: invoice.AmountDue);

        using var stream = new MemoryStream();
        descriptor.Save(stream, ZUGFeRDVersion.Version23, MapProfile(invoice.Profile));
        stream.Position = 0;

        using var reader = new StreamReader(stream, System.Text.Encoding.UTF8);
        return reader.ReadToEnd();
    }

    private static CurrencyCodes MapCurrencyCode(string code) =>
        Enum.TryParse<CurrencyCodes>(code, ignoreCase: true, out CurrencyCodes result)
            ? result
            : CurrencyCodes.EUR;

    private static Profile MapProfile(FacturXProfile profile) => profile switch
    {
        FacturXProfile.Minimum => Profile.Minimum,
        FacturXProfile.BasicWl => Profile.BasicWL,
        FacturXProfile.Basic => Profile.Basic,
        FacturXProfile.En16931 => Profile.Comfort,
        FacturXProfile.Extended => Profile.Extended,
        _ => Profile.Comfort
    };

    private static CountryCodes MapCountryCode(string countryCode) =>
        Enum.TryParse<CountryCodes>(countryCode, ignoreCase: true, out CountryCodes result)
            ? result
            : CountryCodes.FR;

    private static PaymentMeansTypeCodes MapPaymentMeansCode(string code) =>
        int.TryParse(code, out int numericCode)
            ? (PaymentMeansTypeCodes)numericCode
            : PaymentMeansTypeCodes.SEPACreditTransfer;

    private static QuantityCodes MapQuantityCode(string unitCode) =>
        Enum.TryParse<QuantityCodes>(unitCode, ignoreCase: true, out QuantityCodes result)
            ? result
            : QuantityCodes.C62;

    private static TaxCategoryCodes MapTaxCategoryCode(VatCategory category) => category switch
    {
        VatCategory.Standard => TaxCategoryCodes.S,
        VatCategory.ZeroRated => TaxCategoryCodes.Z,
        VatCategory.Exempt => TaxCategoryCodes.E,
        _ => TaxCategoryCodes.S
    };

    private static ElectronicAddressSchemeIdentifiers MapElectronicAddressScheme(string? scheme) =>
        scheme?.ToUpperInvariant() switch
        {
            "EM" => (ElectronicAddressSchemeIdentifiers)int.MaxValue, // Fallback, see below
            "0002" => (ElectronicAddressSchemeIdentifiers)2,
            "0009" => (ElectronicAddressSchemeIdentifiers)9,
            "0088" => (ElectronicAddressSchemeIdentifiers)88,
            _ => (ElectronicAddressSchemeIdentifiers)182 // EM = 182 per CEF code list
        };

    public string Write(CreditNote creditNote)
    {
        var descriptor = InvoiceDescriptor.CreateInvoice(
            creditNote.Number,
            creditNote.IssueDate.ToDateTime(TimeOnly.MinValue),
            MapCurrencyCode(creditNote.CurrencyCode));

        descriptor.Type = InvoiceType.CreditNote;
        descriptor.ReferenceOrderNo = string.Empty;

        if (!string.IsNullOrWhiteSpace(creditNote.BuyerReference))
        {
            descriptor.SetBuyerOrderReferenceDocument(creditNote.BuyerReference);
        }

        // Facture d'origine (BT-25 / BT-26)
        descriptor.AddInvoiceReferencedDocument(
            creditNote.OriginalInvoice.Number,
            creditNote.OriginalInvoice.IssueDate?.ToDateTime(TimeOnly.MinValue));

        // Vendeur (BG-4)
        descriptor.SetSeller(
            creditNote.Seller.Name,
            creditNote.Seller.Address.PostalCode,
            creditNote.Seller.Address.City,
            creditNote.Seller.Address.Line1,
            MapCountryCode(creditNote.Seller.Address.CountryCode),
            legalOrganization: creditNote.Seller.HasLegalOrganizationId ? new LegalOrganization(GlobalIDSchemeIdentifiers.Sirene, creditNote.Seller.LegalOrganizationId) : null);

        if (!string.IsNullOrWhiteSpace(creditNote.Seller.VatIdentifier))
        {
            descriptor.AddSellerTaxRegistration(creditNote.Seller.VatIdentifier, TaxRegistrationSchemeID.VA);
        }

        if (!string.IsNullOrWhiteSpace(creditNote.Seller.LegalRegistrationId))
        {
            descriptor.AddSellerTaxRegistration(creditNote.Seller.LegalRegistrationId, TaxRegistrationSchemeID.FC);
        }

        if (!string.IsNullOrWhiteSpace(creditNote.Seller.ElectronicAddress))
        {
            descriptor.SetSellerElectronicAddress(
                creditNote.Seller.ElectronicAddress,
                MapElectronicAddressScheme(creditNote.Seller.ElectronicAddressScheme));
        }

        // Acheteur (BG-7)
        descriptor.SetBuyer(
            creditNote.Buyer.Name,
            creditNote.Buyer.Address.PostalCode,
            creditNote.Buyer.Address.City,
            creditNote.Buyer.Address.Line1,
            MapCountryCode(creditNote.Buyer.Address.CountryCode),
            legalOrganization: creditNote.Buyer.HasLegalOrganizationId ? new LegalOrganization(GlobalIDSchemeIdentifiers.Sirene, creditNote.Buyer.LegalOrganizationId) : null);

        if (!string.IsNullOrWhiteSpace(creditNote.Buyer.VatIdentifier))
        {
            descriptor.AddBuyerTaxRegistration(creditNote.Buyer.VatIdentifier, TaxRegistrationSchemeID.VA);
        }

        if (!string.IsNullOrWhiteSpace(creditNote.Buyer.LegalRegistrationId))
        {
            descriptor.AddBuyerTaxRegistration(creditNote.Buyer.LegalRegistrationId, TaxRegistrationSchemeID.FC);
        }

        if (!string.IsNullOrWhiteSpace(creditNote.Buyer.ElectronicAddress))
        {
            descriptor.SetBuyerElectronicAddress(
                creditNote.Buyer.ElectronicAddress,
                MapElectronicAddressScheme(creditNote.Buyer.ElectronicAddressScheme));
        }

        // Livraison (BG-13)
        descriptor.ActualDeliveryDate = creditNote.IssueDate.ToDateTime(TimeOnly.MinValue);

        // Moyen de paiement (obligatoire Chorus Pro même sur un avoir)
        descriptor.SetPaymentMeans(
            MapPaymentMeansCode(creditNote.PaymentMeansCode),
            string.Empty);

        // Lignes d'avoir (BG-25)
        foreach (CreditNoteLine line in creditNote.Lines)
        {
            int index = creditNote.Lines.ToList().IndexOf(line) + 1;
            descriptor.AddTradeLineItem(
                lineID: index.ToString(CultureInfo.InvariantCulture),
                name: line.Description,
                billedQuantity: line.Quantity,
                unitCode: MapQuantityCode(line.UnitCode),
                netUnitPrice: line.UnitNetPrice,
                categoryCode: MapTaxCategoryCode(line.VatCategory),
                taxPercent: line.VatRate,
                taxType: TaxTypes.VAT);
        }

        // Ventilation TVA (BG-23)
        foreach (VatBreakdown breakdown in creditNote.VatBreakdowns)
        {
            descriptor.AddApplicableTradeTax(
                breakdown.BasisAmount,
                breakdown.Rate,
                breakdown.TaxAmount,
                TaxTypes.VAT,
                MapTaxCategoryCode(breakdown.Category));
        }

        // Totaux
        descriptor.SetTotals(
            lineTotalAmount: creditNote.TotalWithoutVat,
            taxBasisAmount: creditNote.TotalWithoutVat,
            taxTotalAmount: creditNote.TotalVat,
            grandTotalAmount: creditNote.TotalWithVat,
            duePayableAmount: creditNote.AmountDue);

        using var stream = new MemoryStream();
        descriptor.Save(stream, ZUGFeRDVersion.Version23, MapProfile(creditNote.Profile));
        stream.Position = 0;

        using var reader = new StreamReader(stream, System.Text.Encoding.UTF8);
        return reader.ReadToEnd();
    }
}
