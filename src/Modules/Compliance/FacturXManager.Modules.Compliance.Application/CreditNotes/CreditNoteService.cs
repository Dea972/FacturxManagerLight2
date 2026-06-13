using System.Globalization;
using FacturXManager.Common.Domain;
using FacturXManager.Modules.Compliance.Application.Abstractions;
using FacturXManager.Modules.Compliance.Application.Abstractions.CreditNotes;
using FacturXManager.Modules.Compliance.Application.Abstractions.Invoices;
using FacturXManager.Modules.Compliance.Application.CreditNotes.GenerateCreditNote;
using FacturXManager.Modules.Compliance.Domain.Common;
using FacturXManager.Modules.Compliance.Domain.CreditNotes;
using FacturXManager.Modules.Compliance.Domain.Invoices;
using MediatR;

namespace FacturXManager.Modules.Compliance.Application.CreditNotes;

/// <summary>
/// Service applicatif d'orchestration : génère un avoir à partir d'une facture existante dans l'applicatif.
/// Assemble les données depuis l'<see cref="Invoice"/> source et délègue à <see cref="GenerateCreditNoteCommand"/>.
/// </summary>
internal sealed class CreditNoteService(ISender sender,
    ICreditNoteBuilder creditNoteBuilder,
    ICrossIndustryInvoiceWriter xmlWriter,
    IFacturXPdfA3BRenderer pdfRenderer) : ICreditNoteService
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
            party.LegalOrganizationId,
            party.Address.Line2,
            party.VatIdentifier,
            party.ElectronicAddress,
            party.ElectronicAddressScheme);

    /// <summary>
    /// Génère un avoir à partir de son modèle scalaire et retourne l'agrégat + ses artefacts.
    /// </summary>
    public async Task<Result<CertifiedCreditNote>> GenerateAsync(
        CreditNoteModel model,
        CancellationToken cancellationToken)
    {
        Result<CreditNote> buildResult = creditNoteBuilder.Build(model);

        if (buildResult.IsFailure)
        {
            return Result.Failure<CertifiedCreditNote>(buildResult.Error);
        }

        CreditNote creditNote = buildResult.Value;

        string ciiXml = xmlWriter.Write(creditNote);

        var renderRequest = new FacturXRenderRequest(
            ciiXml,
            FacturXConstants.EmbeddedXmlFileName,
            creditNote.Number,
            creditNote.Seller.Name,
            creditNote.Buyer.Name,
            creditNote.IssueDate,
            creditNote.CurrencyCode,
            creditNote.TotalWithoutVat,
            creditNote.TotalVat,
            creditNote.TotalWithVat,
            creditNote.Profile,
            SellerAddress: FormatAddress(creditNote.Seller.Address),
            SellerVatIdentifier: creditNote.Seller.VatIdentifier,
            SellerLegalRegistrationId: creditNote.Seller.LegalRegistrationId,
            BuyerAddress: FormatAddress(creditNote.Buyer.Address),
            BuyerVatIdentifier: creditNote.Buyer.VatIdentifier,
            BuyerLegalRegistrationId: creditNote.Buyer.LegalRegistrationId,
            AmountDue: creditNote.AmountDue,
            Lines: creditNote.Lines.Select(l =>
            {
                int index = creditNote.Lines.ToList().IndexOf(l) + 1; // 1-based index for line IDs
                var line = new RenderLineItem(
                index.ToString(CultureInfo.InvariantCulture), l.Description, l.Quantity, l.UnitCode, l.UnitNetPrice, l.VatRate, l.NetAmount);
                return line;
            }).ToList(),
            VatBreakdowns: creditNote.VatBreakdowns.Select(v => new RenderVatBreakdown(
                v.Category.ToString(), v.Rate, v.BasisAmount, v.TaxAmount)
            ).ToList());

        Result<byte[]> pdfResult = await pdfRenderer.RenderAsync(renderRequest, cancellationToken);
        if (pdfResult.IsFailure)
        {
            return Result.Failure<CertifiedCreditNote>(pdfResult.Error);
        }

        creditNote.MarkCertified();

        return new CertifiedCreditNote(creditNote, ciiXml, pdfResult.Value);
    }

    private static string FormatAddress(PostalAddress address) =>
        string.IsNullOrWhiteSpace(address.Line2)
            ? $"{address.Line1}, {address.PostalCode} {address.City}, {address.CountryCode}"
            : $"{address.Line1}, {address.Line2}, {address.PostalCode} {address.City}, {address.CountryCode}";
}

/// <summary>
/// Avoir certifié : agrégat + XML CII + octets PDF/A-3b.
/// </summary>
public sealed record CertifiedCreditNote(CreditNote CreditNote, string CrossIndustryInvoiceXml, byte[] PdfBytes);

/// <summary>
/// Paramètres pour la génération d'un avoir depuis une facture existante.
/// </summary>
public sealed record GenerateCreditNoteFromInvoiceRequest(
    Invoice Invoice,
    string CreditNoteNumber,
    DateOnly IssueDate,
    IReadOnlyList<CreditNoteLineModel> Lines,
    string? CreditReason = null);
