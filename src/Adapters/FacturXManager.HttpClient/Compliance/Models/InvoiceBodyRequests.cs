namespace FacturXManager.Client.Compliance.Models;

public sealed class ValidateInvoiceRequest
{
    public InvoiceRequest Invoice { get; set; } = new();
}

public sealed class GenerateInvoiceRequest
{
    public InvoiceRequest Invoice { get; set; } = new();
}
