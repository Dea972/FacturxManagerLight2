using System;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FacturXManager.BackOffice.Web.Features.Invoices.ViewModels;
using Microsoft.AspNetCore.Components.Forms;

namespace FacturXManager.BackOffice.Web.Features.Invoices.ImportFromJson;

/// <summary>
/// Lit un fichier JSON uploadé par l'utilisateur et préremplie un <see cref="InvoiceFormViewModel"/>.
/// </summary>
internal static class InvoiceJsonImporter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    /// <summary>
    /// Taille maximale autorisée pour un JSON de facture (1 Mo).
    /// </summary>
    private const long MaxJsonSizeBytes = 1L * 1024 * 1024;

    /// <summary>
    /// Désérialise le fichier JSON et injecte les valeurs dans le formulaire cible.
    /// Retourne null en cas de succès, ou un message d'erreur lisible par l'utilisateur.
    /// </summary>
    public static async Task<string?> TryApplyAsync(
        IBrowserFile file,
        InvoiceFormViewModel form,
        CancellationToken cancellationToken = default)
    {
        InvoiceImportRoot root;

        try
        {
            await using Stream stream = file.OpenReadStream(MaxJsonSizeBytes, cancellationToken);
            root = await JsonSerializer.DeserializeAsync<InvoiceImportRoot>(stream, JsonOptions, cancellationToken)
                   ?? throw new InvalidOperationException("Fichier JSON vide.");
        }
        catch (Exception ex)
        {
            return $"Impossible de lire le fichier : {ex.Message}";
        }

        InvoiceImportJson invoice = root.Invoice;

        if (!string.IsNullOrWhiteSpace(invoice.Number))
        {
            form.Number = invoice.Number;
        }

        if (!string.IsNullOrWhiteSpace(invoice.IssueDate)
            && DateTime.TryParse(invoice.IssueDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime issueDate))
        {
            form.IssueDate = issueDate;
        }

        if (!string.IsNullOrWhiteSpace(invoice.CurrencyCode))
        {
            form.CurrencyCode = invoice.CurrencyCode;
        }

        if (!string.IsNullOrWhiteSpace(invoice.Profile))
        {
            form.Profile = invoice.Profile;
        }

        if (!string.IsNullOrWhiteSpace(invoice.OperationNature))
        {
            form.OperationNature = invoice.OperationNature;
        }

        form.BuyerReference = invoice.BuyerReference;
        form.PurchaseOrderReference = invoice.PurchaseOrderReference;

        if (invoice.Seller is not null)
        {
            ApplyParty(invoice.Seller, form.Seller);
        }

        if (invoice.Buyer is not null)
        {
            ApplyParty(invoice.Buyer, form.Buyer);
        }

        if (invoice.Payment is not null)
        {
            if (!string.IsNullOrWhiteSpace(invoice.Payment.MeansCode))
            {
                form.Payment.MeansCode = MapPaymentMeansCode(invoice.Payment.MeansCode);
            }

            if (!string.IsNullOrWhiteSpace(invoice.Payment.DueDate)
                && DateTime.TryParse(invoice.Payment.DueDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dueDate))
            {
                form.Payment.DueDate = dueDate;
            }

            form.Payment.Iban = invoice.Payment.Iban;
        }

        if (invoice.Lines is { Count: > 0 })
        {
            form.Lines.Clear();
            foreach (LineImportJson line in invoice.Lines)
            {
                form.Lines.Add(new InvoiceLineFormViewModel
                {
                    Id = line.Id ?? (form.Lines.Count + 1).ToString(CultureInfo.InvariantCulture),
                    Description = line.Description ?? string.Empty,
                    Quantity = line.Quantity,
                    UnitNetPrice = line.UnitNetPrice,
                    VatCategory = line.VatCategory ?? "Standard",
                    VatRate = line.VatRate,
                    UnitCode = line.UnitCode ?? "C62",
                });
            }
        }

        return null;
    }

    internal static void ApplyParty(PartyImportJson src, PartyFormViewModel dest)
    {
        if (!string.IsNullOrWhiteSpace(src.Name))
        {
            dest.Name = src.Name;
        }

        if (!string.IsNullOrWhiteSpace(src.AddressLine1))
        {
            dest.AddressLine1 = src.AddressLine1;
        }

        dest.AddressLine2 = src.AddressLine2;

        if (!string.IsNullOrWhiteSpace(src.PostalCode))
        {
            dest.PostalCode = src.PostalCode;
        }

        if (!string.IsNullOrWhiteSpace(src.City))
        {
            dest.City = src.City;
        }

        if (!string.IsNullOrWhiteSpace(src.CountryCode))
        {
            dest.CountryCode = src.CountryCode;
        }

        dest.LegalOrganizationId = src.LegalOrganizationId;
        dest.VatIdentifier = src.VatIdentifier;
    }

    /// <summary>
    /// Le JSON peut contenir le code numérique UNTDID 4461 (ex: "30") ou le libellé applicatif
    /// (ex: "BankTransfer"). On normalise vers le libellé attendu par le formulaire.
    /// </summary>
    internal static string MapPaymentMeansCode(string raw) => raw switch
    {
        "30" or "BankTransfer" => "BankTransfer",
        "31" or "DebitTransfer" => "DebitTransfer",
        "48" or "BankCard" => "BankCard",
        "49" or "DirectDebit" => "DirectDebit",
        "58" or "SepaCreditTransfer" => "SepaCreditTransfer",
        "59" or "SepaDirectDebit" => "SepaDirectDebit",
        "97" or "ClearingBetweenPartners" => "ClearingBetweenPartners",
        _ => raw,
    };
}
