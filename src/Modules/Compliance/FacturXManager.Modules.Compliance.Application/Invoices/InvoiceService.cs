using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using FacturXManager.Common.Domain;
using FacturXManager.Modules.Compliance.Application.Abstractions;
using FacturXManager.Modules.Compliance.Application.Abstractions.Invoices;
using FacturXManager.Modules.Compliance.Domain.Common;
using FacturXManager.Modules.Compliance.Domain.Invoices;

namespace FacturXManager.Modules.Compliance.Application.Invoices;

internal sealed class InvoiceService(
    IInvoiceBuilder invoiceBuilder,
    IEn16931Validator validator,
    ICrossIndustryInvoiceWriter xmlWriter,
    IFacturXPdfA3BRenderer pdfRenderer) : IInvoiceService
{

    public async Task<Result<CertifiedInvoice>> GenerateAsync(
        InvoiceModel model,
        CancellationToken cancellationToken = default)
    {
        Result<Invoice> buildResult = invoiceBuilder.Build(model);
        if (buildResult.IsFailure)
        {
            return Result.Failure<CertifiedInvoice>(buildResult.Error);
        }

        Invoice invoice = buildResult.Value;

        Result validation = validator.Validate(invoice);
        if (validation.IsFailure)
        {
            return Result.Failure<CertifiedInvoice>(validation.Error);
        }

        string ciiXml = xmlWriter.Write(invoice);

        var renderRequest = new FacturXRenderRequest(
            ciiXml,
            FacturXConstants.EmbeddedXmlFileName,
            invoice.Number,
            invoice.Seller.Name,
            invoice.Buyer.Name,
            invoice.IssueDate,
            invoice.CurrencyCode,
            invoice.TotalWithoutVat,
            invoice.TotalVat,
            invoice.TotalWithVat,
            invoice.Profile,
            SellerAddress: FormatAddress(invoice.Seller.Address),
            SellerVatIdentifier: invoice.Seller.VatIdentifier,
            SellerLegalRegistrationId: invoice.Seller.LegalRegistrationId,
            BuyerAddress: FormatAddress(invoice.Buyer.Address),
            BuyerVatIdentifier: invoice.Buyer.VatIdentifier,
            BuyerLegalRegistrationId: invoice.Buyer.LegalRegistrationId,
            AmountDue: invoice.AmountDue,
            PaymentMeansCode: invoice.Payment?.MeansCode,
            PaymentIban: invoice.Payment?.Iban,
            PaymentDueDate: invoice.Payment?.DueDate,
            Lines: invoice.Lines.Select(l =>
            {
                int index = invoice.Lines.ToList().IndexOf(l) + 1; // 1-based index for line IDs
                var line = new RenderLineItem(
                    index.ToString(CultureInfo.InvariantCulture), l.Description, l.Quantity, l.UnitCode, l.UnitNetPrice, l.VatRate, l.NetAmount);
                return line;
            }).ToList(),
            VatBreakdowns: invoice.VatBreakdowns.Select(v => new RenderVatBreakdown(
                v.Category.ToString(), v.Rate, v.BasisAmount, v.TaxAmount)).ToList());

        Result<byte[]> pdfResult = await pdfRenderer.RenderAsync(renderRequest, cancellationToken);
        if (pdfResult.IsFailure)
        {
            return Result.Failure<CertifiedInvoice>(pdfResult.Error);
        }

        invoice.MarkCertified();

        return new CertifiedInvoice(invoice, ciiXml, pdfResult.Value);
    }

    private static string FormatAddress(PostalAddress address) =>
        string.IsNullOrWhiteSpace(address.Line2)
            ? $"{address.Line1}, {address.PostalCode} {address.City}, {address.CountryCode}"
            : $"{address.Line1}, {address.Line2}, {address.PostalCode} {address.City}, {address.CountryCode}";
}

/// <summary>
/// Facture certifiée : agrégat + XML CII + octets PDF/A-3b.
/// </summary>
public sealed record CertifiedInvoice(Invoice Invoice, string CrossIndustryInvoiceXml, byte[] PdfBytes);
