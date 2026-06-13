using System.Collections.Generic;
using System.Text.Json.Serialization;
using FacturXManager.BackOffice.Web.Features.Invoices.ImportFromJson;

namespace FacturXManager.BackOffice.Web.Features.CreditNotes.ImportFromJson;

internal sealed class CreditNoteImportRoot
{
    [JsonPropertyName("creditNote")]
    public CreditNoteImportJson CreditNote { get; set; } = new();
}

internal sealed class CreditNoteImportJson
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

    [JsonPropertyName("originalInvoiceNumber")]
    public string? OriginalInvoiceNumber { get; set; }

    [JsonPropertyName("originalInvoiceIssueDate")]
    public string? OriginalInvoiceIssueDate { get; set; }

    [JsonPropertyName("creditReason")]
    public string? CreditReason { get; set; }

    [JsonPropertyName("buyerReference")]
    public string? BuyerReference { get; set; }

    [JsonPropertyName("paymentMeansCode")]
    public string? PaymentMeansCode { get; set; }

    [JsonPropertyName("seller")]
    public PartyImportJson? Seller { get; set; }

    [JsonPropertyName("buyer")]
    public PartyImportJson? Buyer { get; set; }

    [JsonPropertyName("lines")]
    public List<LineImportJson>? Lines { get; set; }
}
