using System;
using System.Collections.Generic;

namespace FacturXManager.Client.EReporting.Models;

public sealed class DeclarationDto
{
    public Guid Id { get; set; }
    public string Period { get; set; } = string.Empty;
    public string TransactionType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal TotalTaxableAmount { get; set; }
    public decimal TotalVatAmount { get; set; }
    public string? DgfipSubmissionReference { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public DateTime? SubmittedOnUtc { get; set; }
    public DateTime? AcceptedOnUtc { get; set; }
    public List<DeclarationEntryDto> Entries { get; set; } = new();
}

public sealed class DeclarationEntryDto
{
    public decimal TaxableAmount { get; set; }
    public decimal VatAmount { get; set; }
    public string CountryCode { get; set; } = string.Empty;
    public string OperationNature { get; set; } = string.Empty;
}
