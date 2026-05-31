using FacturXManager.Common.Domain;
using FacturXManager.Modules.Compliance.Application.Abstractions;
using FacturXManager.Modules.Compliance.Domain.Invoicing;

namespace FacturXManager.Modules.Compliance.Infrastructure.Invoicing;

internal sealed class InvoiceBuilder : IInvoiceBuilder
{
    public Result<Invoice> Build(InvoiceModel model)
    {
        if (!TryParseProfile(model.Profile, out FacturXProfile profile))
        {
            return Result.Failure<Invoice>(InvoiceErrors.InvalidProfile(model.Profile));
        }

        if (!TryParseOperationNature(model.OperationNature, out OperationNature operationNature))
        {
            return Result.Failure<Invoice>(InvoiceErrors.InvalidOperationNature(model.OperationNature));
        }

        var lines = new List<InvoiceLine>(model.Lines.Count);
        foreach (InvoiceLineModel lineModel in model.Lines)
        {
            if (!TryParseVatCategory(lineModel.VatCategory, out VatCategory category))
            {
                return Result.Failure<Invoice>(InvoiceErrors.InvalidVatCategory(lineModel.VatCategory));
            }

            lines.Add(InvoiceLine.Create(
                lineModel.Id,
                lineModel.Description,
                lineModel.Quantity,
                lineModel.UnitNetPrice,
                category,
                lineModel.VatRate,
                lineModel.UnitCode));
        }

        Party seller = ToParty(model.Seller);
        Party buyer = ToParty(model.Buyer);
        var payment = PaymentDetails.Create(model.Payment.MeansCode, model.Payment.DueDate, model.Payment.Iban);

        return Invoice.Create(
            model.Number,
            model.IssueDate,
            model.CurrencyCode,
            profile,
            operationNature,
            seller,
            buyer,
            payment,
            lines,
            model.BuyerReference,
            model.PurchaseOrderReference);
    }

    private static Party ToParty(PartyModel model)
    {
        var address = PostalAddress.Create(
            model.AddressLine1,
            model.PostalCode,
            model.City,
            model.CountryCode,
            model.AddressLine2);

        return Party.Create(model.Name, address, model.LegalRegistrationId, model.LegalOrganizationId, model.VatIdentifier, model.ElectronicAddress, model.ElectronicAddressScheme);
    }

    private static bool TryParseProfile(string value, out FacturXProfile profile)
    {
        switch (value?.Trim().ToUpperInvariant())
        {
            case "EN16931" or "COMFORT":
                profile = FacturXProfile.En16931;
                return true;
            case "EXTENDED":
                profile = FacturXProfile.Extended;
                return true;
            case "BASIC":
                profile = FacturXProfile.Basic;
                return true;
            case "BASICWL":
                profile = FacturXProfile.BasicWl;
                return true;
            case "MINIMUM":
                profile = FacturXProfile.Minimum;
                return true;
            default:
                profile = default;
                return false;
        }
    }

    private static bool TryParseOperationNature(string value, out OperationNature nature) =>
        Enum.TryParse(value?.Trim(), ignoreCase: true, out nature) && Enum.IsDefined(nature);

    private static bool TryParseVatCategory(string value, out VatCategory category) =>
        Enum.TryParse(value?.Trim(), ignoreCase: true, out category) && Enum.IsDefined(category);
}
