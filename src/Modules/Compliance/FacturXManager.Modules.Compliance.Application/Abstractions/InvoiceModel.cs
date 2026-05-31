namespace FacturXManager.Modules.Compliance.Application.Abstractions;

/// <summary>
/// Représentation scalaire d'une facture, partagée par les use cases du module.
/// Aucun type Domain n'apparaît ici : le builder instancie les types Domain à partir de ces scalaires.
/// </summary>
public sealed record InvoiceModel(
    string Number,
    DateOnly IssueDate,
    string CurrencyCode,
    string Profile,
    string OperationNature,
    PartyModel Seller,
    PartyModel Buyer,
    PaymentModel Payment,
    IReadOnlyList<InvoiceLineModel> Lines,
    string? BuyerReference = null,
    string? PurchaseOrderReference = null);

public sealed record PartyModel(
    string Name,
    string AddressLine1,
    string PostalCode,
    string City,
    string CountryCode,
    string? AddressLine2 = null,
    string? LegalRegistrationId = null,
    string? LegalOrganizationId = null,
    string? VatIdentifier = null,
    string? ElectronicAddress = null,
    string? ElectronicAddressScheme = null);

public sealed record PaymentModel(
    PaymentMeansCode MeansCode,
    DateOnly? DueDate = null,
    string? Iban = null);

public sealed record InvoiceLineModel(
    string Id,
    string Description,
    decimal Quantity,
    decimal UnitNetPrice,
    string VatCategory,
    decimal VatRate,
    string UnitCode = "C62");
