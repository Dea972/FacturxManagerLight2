using System.ComponentModel.DataAnnotations;

namespace FacturXManager.BackOffice.Web.Features.Invoices.ViewModels;

/// <summary>
/// ViewModel éditable d'une facture, lié aux formulaires Radzen.
/// Mappé vers les DTOs Refit par les handlers (logique de vue uniquement).
/// </summary>
public sealed class InvoiceFormViewModel
{
    [Required(ErrorMessage = "Le numéro de facture est obligatoire.")]
    public string Number { get; set; } = string.Empty;

    [Required]
    public DateTime IssueDate { get; set; } = DateTime.Today;

    [Required]
    public string CurrencyCode { get; set; } = "EUR";

    [Required]
    public string Profile { get; set; } = "EN16931";

    [Required]
    public string OperationNature { get; set; } = "Goods";

    public string? BuyerReference { get; set; }

    public string? PurchaseOrderReference { get; set; }

    public PartyFormViewModel Seller { get; set; } = new();

    public PartyFormViewModel Buyer { get; set; } = new();

    public PaymentFormViewModel Payment { get; set; } = new();

    public List<InvoiceLineFormViewModel> Lines { get; set; } = [new()];
}

public sealed class PartyFormViewModel
{
    [Required(ErrorMessage = "La raison sociale est obligatoire.")]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string AddressLine1 { get; set; } = string.Empty;

    public string? AddressLine2 { get; set; }

    [Required]
    public string PostalCode { get; set; } = string.Empty;

    [Required]
    public string City { get; set; } = string.Empty;

    [Required]
    public string CountryCode { get; set; } = "FR";

    /// <summary>SIRET (14 chiffres) saisi dans le formulaire.</summary>
    public string LegalOrganizationId { get; set; }

    /// <summary>SIREN calculé à partir des 9 premiers caractères du SIRET.</summary>
    public string? LegalRegistrationId =>
        LegalOrganizationId is { Length: >= 9 } siret ? siret[..9] : null;

    public string? VatIdentifier { get; set; }
}

public sealed class PaymentFormViewModel
{
    [Required]
    public string MeansCode { get; set; } = "BankTransfer";

    public DateTime? DueDate { get; set; }

    public string? Iban { get; set; }
}

public sealed class InvoiceLineFormViewModel
{
    [Required]
    public string Id { get; set; } = "1";

    [Required(ErrorMessage = "La désignation est obligatoire.")]
    public string Description { get; set; } = string.Empty;

    public decimal Quantity { get; set; } = 1m;

    public decimal UnitNetPrice { get; set; }

    public string VatCategory { get; set; } = "Standard";

    public decimal VatRate { get; set; } = 20m;

    public string UnitCode { get; set; } = "C62";
}
