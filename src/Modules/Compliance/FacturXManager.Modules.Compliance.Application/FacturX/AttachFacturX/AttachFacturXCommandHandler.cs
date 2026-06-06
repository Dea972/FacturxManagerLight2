using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FacturXManager.Common.Application.Messaging;
using FacturXManager.Common.Domain;
using FacturXManager.Modules.Compliance.Application.Abstractions;
using FacturXManager.Modules.Compliance.Application.Abstractions.Invoices;
using FacturXManager.Modules.Compliance.Domain.Common;
using FacturXManager.Modules.Compliance.Domain.Invoices;

namespace FacturXManager.Modules.Compliance.Application.FacturX.AttachFacturX;

public sealed class AttachFacturXCommandHandler(
    IInvoiceBuilder invoiceBuilder,
    IEn16931Validator validator,
    ICrossIndustryInvoiceWriter xmlWriter,
    IFacturXPdfEmbedder pdfEmbedder)
    : ICommandHandler<AttachFacturXCommand, AttachFacturXSummary>
{
    public async Task<Result<AttachFacturXSummary>> Handle(
        AttachFacturXCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Convertir et valider la conformité des données d'Invoice
        Result<Invoice> buildResult = invoiceBuilder.Build(request.Invoice);
        if (buildResult.IsFailure)
        {
            return Result.Failure<AttachFacturXSummary>(buildResult.Error);
        }

        Invoice invoice = buildResult.Value;

        Result validation = validator.Validate(invoice);
        if (validation.IsFailure)
        {
            return Result.Failure<AttachFacturXSummary>(validation.Error);
        }

        // 2. Générer le XML CII réglementaire
        string ciiXml = xmlWriter.Write(invoice);

        // 3. Modéliser la requête d'intégration (réutilisation de FacturXRenderRequest existant)
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
            PaymentMeansCode: invoice.Payment.MeansCode,
            PaymentIban: invoice.Payment.Iban,
            PaymentDueDate: invoice.Payment.DueDate,
            Lines: invoice.Lines.Select((l, index) =>
                new RenderLineItem(
                    (index + 1).ToString(CultureInfo.InvariantCulture),
                    l.Description,
                    l.Quantity,
                    l.UnitCode,
                    l.UnitNetPrice,
                    l.VatRate,
                    l.NetAmount)).ToList(),
            VatBreakdowns: invoice.VatBreakdowns.Select(v =>
                new RenderVatBreakdown(
                    v.Category.ToString(),
                    v.Rate,
                    v.BasisAmount,
                    v.TaxAmount)).ToList());

        // 4. Embarquer le XML CII et les métadonnées XMP dans le PDF existant fourni
        Result<byte[]> pdfResult = await pdfEmbedder.EmbedAsync(request.VisualPdfBytes, renderRequest, cancellationToken);
        if (pdfResult.IsFailure)
        {
            return Result.Failure<AttachFacturXSummary>(pdfResult.Error);
        }

        return new AttachFacturXSummary(
            invoice.Number,
            invoice.Profile.ToString(),
            FacturXConstants.EmbeddedXmlFileName,
            ciiXml,
            Convert.ToBase64String(pdfResult.Value),
            invoice.TotalWithVat);
    }

    private static string FormatAddress(PostalAddress address) =>
        string.IsNullOrWhiteSpace(address.Line2)
            ? $"{address.Line1}, {address.PostalCode} {address.City}, {address.CountryCode}"
            : $"{address.Line1}, {address.Line2}, {address.PostalCode} {address.City}, {address.CountryCode}";
}
