using System;
using System.Collections.Generic;

namespace FacturXManager.Client.Compliance.Models;

public sealed class InvoiceRequest
{
    public string Number { get; set; } = string.Empty;
    public string IssueDate { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = string.Empty;
    public string Profile { get; set; } = string.Empty;
    public string OperationNature { get; set; } = string.Empty;
    public PartyRequest Seller { get; set; } = new();
    public PartyRequest Buyer { get; set; } = new();
    public PaymentRequest Payment { get; set; } = new();
    public List<InvoiceLineRequest> Lines { get; set; } = new();
    public string? BuyerReference { get; set; }
    public string? PurchaseOrderReference { get; set; }
}

public sealed class PartyRequest
{
    public string Name { get; set; } = string.Empty;
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string PostalCode { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public string? LegalRegistrationId { get; set; }
    public string? VatIdentifier { get; set; }
}

public sealed class PaymentRequest
{
    public string MeansCode { get; set; } = string.Empty;
    public string? DueDate { get; set; }
    public string? Iban { get; set; }
}

public sealed class InvoiceLineRequest
{
    public string Id { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitNetPrice { get; set; }
    public string VatCategory { get; set; } = string.Empty;
    public decimal VatRate { get; set; }
    public string UnitCode { get; set; } = "C62";
}
