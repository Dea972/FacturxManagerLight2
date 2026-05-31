using FacturXManager.Common.Domain;
using FacturXManager.Modules.Compliance.Application.Abstractions;
using FacturXManager.Modules.Compliance.Application.Invoicing.GenerateCreditNote;
using FacturXManager.Modules.Compliance.Domain.Invoicing;
using MediatR;

namespace FacturXManager.Modules.Compliance.Application.Invoicing;

/// <summary>
/// Service applicatif d'orchestration : génère un avoir à partir d'une facture existante dans l'applicatif.
/// Assemble les données depuis l'<see cref="Invoice"/> source et délègue à <see cref="GenerateCreditNoteCommand"/>.
/// </summary>
public sealed class CreditNoteService(ISender sender)
{
    /// <summary>
    /// Génère un avoir Factur-X à partir d'une facture existante et des paramètres de l'avoir.
    /// </summary>
    public async Task<Result<GenerateCreditNoteSummary>> GenerateFromInvoiceAsync(
        GenerateCreditNoteFromInvoiceRequest request,
        CancellationToken cancellationToken = default)
    {
        Invoice invoice = request.Invoice;

        var creditNoteModel = new CreditNoteModel(
            Number: request.CreditNoteNumber,
            IssueDate: request.IssueDate,
            CurrencyCode: invoice.CurrencyCode,
            Profile: invoice.Profile.ToString(),
            OperationNature: invoice.OperationNature.ToString(),
            Seller: ToPartyModel(invoice.Seller),
            Buyer: ToPartyModel(invoice.Buyer),
            OriginalInvoiceNumber: invoice.Number,
            Lines: request.Lines,
            OriginalInvoiceIssueDate: invoice.IssueDate,
            CreditReason: request.CreditReason,
            BuyerReference: invoice.BuyerReference);

        var command = new GenerateCreditNoteCommand(creditNoteModel);

        return await sender.Send(command, cancellationToken);
    }

    private static PartyModel ToPartyModel(Party party) =>
        new(
            party.Name,
            party.Address.Line1,
            party.Address.PostalCode,
            party.Address.City,
            party.Address.CountryCode,
            party.Address.Line2,
            party.LegalRegistrationId,
            party.LegalOrganizationId,
            party.VatIdentifier,
            party.ElectronicAddress,
            party.ElectronicAddressScheme);
}

/// <summary>
/// Paramètres pour la génération d'un avoir depuis une facture existante.
/// </summary>
public sealed record GenerateCreditNoteFromInvoiceRequest(
    Invoice Invoice,
    string CreditNoteNumber,
    DateOnly IssueDate,
    IReadOnlyList<CreditNoteLineModel> Lines,
    string? CreditReason = null);
