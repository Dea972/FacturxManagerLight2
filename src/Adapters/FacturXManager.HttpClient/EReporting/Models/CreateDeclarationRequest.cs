namespace FacturXManager.Client.EReporting.Models;

public sealed class CreateDeclarationRequest
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string TransactionType { get; set; } = string.Empty;
}
