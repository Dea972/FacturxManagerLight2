using System;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FacturXManager.BackOffice.Web.Features.CreditNotes.ViewModels;
using FacturXManager.BackOffice.Web.Features.Invoices.ImportFromJson;
using Microsoft.AspNetCore.Components.Forms;

namespace FacturXManager.BackOffice.Web.Features.CreditNotes.ImportFromJson;

/// <summary>
/// Lit un fichier JSON uploadé par l'utilisateur et préremplie un <see cref="CreditNoteFormViewModel"/>.
/// </summary>
internal static class CreditNoteJsonImporter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private const long MaxJsonSizeBytes = 1L * 1024 * 1024;

    /// <summary>
    /// Désérialise le fichier JSON et injecte les valeurs dans le formulaire cible.
    /// Retourne null en cas de succès, ou un message d'erreur lisible par l'utilisateur.
    /// </summary>
    public static async Task<string?> TryApplyAsync(
        IBrowserFile file,
        CreditNoteFormViewModel form,
        CancellationToken cancellationToken = default)
    {
        CreditNoteImportRoot root;

        try
        {
            await using Stream stream = file.OpenReadStream(MaxJsonSizeBytes, cancellationToken);
            root = await JsonSerializer.DeserializeAsync<CreditNoteImportRoot>(stream, JsonOptions, cancellationToken)
                   ?? throw new InvalidOperationException("Fichier JSON vide.");
        }
        catch (Exception ex)
        {
            return $"Impossible de lire le fichier : {ex.Message}";
        }

        CreditNoteImportJson creditNote = root.CreditNote;

        if (!string.IsNullOrWhiteSpace(creditNote.Number))
        {
            form.Number = creditNote.Number;
        }

        if (!string.IsNullOrWhiteSpace(creditNote.IssueDate)
            && DateTime.TryParse(creditNote.IssueDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime issueDate))
        {
            form.IssueDate = issueDate;
        }

        if (!string.IsNullOrWhiteSpace(creditNote.CurrencyCode))
        {
            form.CurrencyCode = creditNote.CurrencyCode;
        }

        if (!string.IsNullOrWhiteSpace(creditNote.Profile))
        {
            form.Profile = creditNote.Profile;
        }

        if (!string.IsNullOrWhiteSpace(creditNote.OperationNature))
        {
            form.OperationNature = creditNote.OperationNature;
        }

        if (!string.IsNullOrWhiteSpace(creditNote.OriginalInvoiceNumber))
        {
            form.OriginalInvoiceNumber = creditNote.OriginalInvoiceNumber;
        }

        if (!string.IsNullOrWhiteSpace(creditNote.OriginalInvoiceIssueDate)
            && DateTime.TryParse(creditNote.OriginalInvoiceIssueDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime originalDate))
        {
            form.OriginalInvoiceIssueDate = originalDate;
        }

        form.CreditReason = creditNote.CreditReason;
        form.BuyerReference = creditNote.BuyerReference;

        if (!string.IsNullOrWhiteSpace(creditNote.PaymentMeansCode))
        {
            form.PaymentMeansCode = InvoiceJsonImporter.MapPaymentMeansCode(creditNote.PaymentMeansCode);
        }

        if (creditNote.Seller is not null)
        {
            InvoiceJsonImporter.ApplyParty(creditNote.Seller, form.Seller);
        }

        if (creditNote.Buyer is not null)
        {
            InvoiceJsonImporter.ApplyParty(creditNote.Buyer, form.Buyer);
        }

        if (creditNote.Lines is { Count: > 0 })
        {
            form.Lines.Clear();
            foreach (LineImportJson line in creditNote.Lines)
            {
                form.Lines.Add(new CreditNoteLineFormViewModel
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
}
