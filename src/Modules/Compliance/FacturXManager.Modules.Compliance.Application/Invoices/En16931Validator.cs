using System.Globalization;
using FacturXManager.Common.Domain;
using FacturXManager.Modules.Compliance.Application.Abstractions;
using FacturXManager.Modules.Compliance.Domain.Common;
using FacturXManager.Modules.Compliance.Domain.Invoices;

namespace FacturXManager.Modules.Compliance.Application.Invoices;

/// <summary>
/// Validation EN16931 (profil Comfort) + obligations françaises (SIREN acheteur, nature d'opération
/// à compter du 1er septembre 2026). Agrège l'ensemble des violations détectées.
/// </summary>
internal sealed class En16931Validator : IEn16931Validator
{
    public Result Validate(Invoice invoice)
    {
        var errors = new List<Error>();

        if (string.IsNullOrWhiteSpace(invoice.Seller.Name))
        {
            errors.Add(InvoiceErrors.SellerRequired);
        }

        if (!invoice.Seller.HasLegalOrganizationId)
        {
            errors.Add(InvoiceErrors.SellerSirenRequired);
        }

        if (invoice.Profile == FacturXProfile.En16931 &&
            string.IsNullOrWhiteSpace(invoice.Seller.VatIdentifier))
        {
            errors.Add(InvoiceErrors.SellerVatIdentifierRequired);
        }

        if (string.IsNullOrWhiteSpace(invoice.Buyer.Name))
        {
            errors.Add(InvoiceErrors.BuyerRequired);
        }

        if (invoice.IsSubjectToB2BObligation() && invoice.OperationNature == OperationNature.Unspecified)
        {
                errors.Add(InvoiceErrors.OperationNatureRequired);
        }

        foreach (InvoiceLine line in invoice.Lines)
        {
            if (string.IsNullOrWhiteSpace(line.Description))
            {
                errors.Add(InvoiceErrors.LineDescriptionRequired(line.Id.ToString(CultureInfo.InvariantCulture)));
            }

            if (line.Quantity <= 0)
            {
                errors.Add(InvoiceErrors.LineQuantityInvalid(line.Id.ToString(CultureInfo.InvariantCulture)));
            }

            if (!line.VatCategory.AllowsPositiveRate() && line.VatRate > 0)
            {
                errors.Add(InvoiceErrors.PositiveRateForExemptCategory(line.VatCategory.ToUnclCode()));
            }
        }

        if (errors.Count > 0)
        {
            return Result.Failure(new ValidationError([.. errors]));
        }

        return Result.Success();
    }
}
