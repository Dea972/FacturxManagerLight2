using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FacturXManager.BackOffice.Web.Features.Invoices.ImportFromJson;

internal sealed class InvoiceImportRoot
{
    [JsonPropertyName("invoice")]
    public InvoiceImportJson Invoice { get; set; } = new();
}

internal sealed class InvoiceImportJson
{
    [JsonPropertyName("number")]
    public string? Number { get; set; }

    [JsonPropertyName("issueDate")]
    public string? IssueDate { get; set; }

    [JsonPropertyName("currencyCode")]
    public string? CurrencyCode { get; set; }

    [JsonPropertyName("profile")]
    public string? Profile { get; set; }

    [JsonPropertyName("operationNature")]
    public string? OperationNature { get; set; }

    [JsonPropertyName("buyerReference")]
    public string? BuyerReference { get; set; }

    [JsonPropertyName("purchaseOrderReference")]
    public string? PurchaseOrderReference { get; set; }

    [JsonPropertyName("seller")]
    public PartyImportJson? Seller { get; set; }

    [JsonPropertyName("buyer")]
    public PartyImportJson? Buyer { get; set; }

    [JsonPropertyName("payment")]
    public PaymentImportJson? Payment { get; set; }

    [JsonPropertyName("lines")]
    public List<LineImportJson>? Lines { get; set; }
}

internal sealed class PartyImportJson
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("addressLine1")]
    public string? AddressLine1 { get; set; }

    [JsonPropertyName("addressLine2")]
    public string? AddressLine2 { get; set; }

    [JsonPropertyName("postalCode")]
    public string? PostalCode { get; set; }

    [JsonPropertyName("city")]
    public string? City { get; set; }

    [JsonPropertyName("countryCode")]
    public string? CountryCode { get; set; }

    [JsonPropertyName("legalRegistrationId")]
    public string? LegalRegistrationId { get; set; }

    [JsonPropertyName("legalOrganizationId")]
    public string LegalOrganizationId { get; set; }

    [JsonPropertyName("vatIdentifier")]
    public string? VatIdentifier { get; set; }
}

internal sealed class PaymentImportJson
{
    [JsonPropertyName("meansCode")]
    public string? MeansCode { get; set; }

    [JsonPropertyName("dueDate")]
    public string? DueDate { get; set; }

    [JsonPropertyName("iban")]
    public string? Iban { get; set; }
}

internal sealed class LineImportJson
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("quantity")]
    public decimal Quantity { get; set; }

    [JsonPropertyName("unitNetPrice")]
    public decimal UnitNetPrice { get; set; }

    [JsonPropertyName("vatCategory")]
    public string? VatCategory { get; set; }

    [JsonPropertyName("vatRate")]
    public decimal VatRate { get; set; }

    [JsonPropertyName("unitCode")]
    public string? UnitCode { get; set; }
}
