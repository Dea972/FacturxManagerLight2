namespace FacturXManager.Client.EReporting.Models;

public sealed class AddDeclarationEntryRequest
{
    public decimal TaxableAmount { get; set; }
    public decimal VatAmount { get; set; }
    public string CountryCode { get; set; } = string.Empty;
    public string OperationNature { get; set; } = string.Empty;
}
