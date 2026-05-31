using System.Globalization;
using System.Text;
using FacturXManager.Common.Domain;
using FacturXManager.Modules.Compliance.Application.Abstractions;
using FacturXManager.Modules.Compliance.Domain.Invoicing;
using FacturXManager.Modules.Compliance.Infrastructure.Invoicing;
using Microsoft.Extensions.Logging;
using QuestPDF;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using static QuestPDF.Fluent.DocumentOperation;

namespace FacturXManager.Modules.Compliance.Infrastructure.QuestPdf;

/// <summary>
/// Implémentation PDF/A-3b basée sur QuestPDF pour le rendu visuel, l'embedding du XML CII
/// (factur-x.xml, AFRelationship=Data) et les métadonnées XMP Factur-X.
/// Utilise les API natives QuestPDF (DocumentSettings.PdfA, DocumentOperation.AddAttachment,
/// DocumentOperation.ExtendMetadata) pour une conformité PDF/A-3b sans PdfSharp.
/// </summary>
internal sealed class QuestPdfFacturXRenderer : IFacturXPdfA3BRenderer
{
    private readonly ILogger<QuestPdfFacturXRenderer> _logger;

    public QuestPdfFacturXRenderer(ILogger<QuestPdfFacturXRenderer> logger)
    {
        _logger = logger;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<Result<byte[]>> RenderAsync(
        FacturXRenderRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            byte[] finalPdf = GenerateFacturXPdf(request);
            return await Task.FromResult(Result.Success(finalPdf));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Échec du rendu QuestPDF/A-3b Factur-X pour la facture {InvoiceNumber}.", request.InvoiceNumber);
            return Result.Failure<byte[]>(InvoiceErrors.PdfRendererNotConfigured);
        }
    }

    private static byte[] GenerateFacturXPdf(FacturXRenderRequest request)
    {
        bool hasLines = request.Lines is { Count: > 0 };

        // Étape 1 : Génère le PDF/A-3b visuel via QuestPDF
        string tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".pdf");
        string outputPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".pdf");
        string xmlTempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".xml");

        try
        {
            // Écrire le XML CII dans un fichier temporaire pour l'attachement
            File.WriteAllBytes(xmlTempPath, Encoding.UTF8.GetBytes(request.CrossIndustryInvoiceXml));

            // Générer le PDF visuel avec PDF/A activé
            Document
                .Create(container => container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.MarginHorizontal(40);
                    page.MarginVertical(50);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Element(header => ComposeHeader(header, request));
                    page.Content().Element(content =>
                    {
                        if (hasLines)
                        {
                            ComposeDetailedContent(content, request);
                        }
                        else
                        {
                            ComposeBasicContent(content, request);
                        }
                    });
                    page.Footer().Element(ComposeFooter);
                }))
                .WithSettings(new DocumentSettings
                {
                    PDFA_Conformance = PDFA_Conformance.PDFA_3B
                })
                .WithMetadata(new DocumentMetadata
                {
                    Title = $"Facture {request.InvoiceNumber}",
                    Author = request.SellerName,
                    Subject = "Facture Factur-X EN16931",
                    Keywords = "Factur-X, EN16931, invoice",
                    Creator = "FacturXManager",
                    Producer = "QuestPDF"
                })
                .GeneratePdf(tempPath);

            // Étape 2 : Attacher le XML CII et étendre les métadonnées XMP
            string facturXMetadata = BuildFacturXXmpExtension(request);

            DocumentOperation
                .LoadFile(tempPath)
                .AddAttachment(new DocumentOperation.DocumentAttachment
                {
                    FilePath = xmlTempPath,
                    Key = request.EmbeddedFileName,
                    AttachmentName = request.EmbeddedFileName,
                    Description = "Factur-X invoice data (Cross Industry Invoice XML)",
                    MimeType = "text/xml",
                    Relationship = DocumentOperation.DocumentAttachmentRelationship.Data,
                    CreationDate = DateTime.UtcNow,
                    ModificationDate = DateTime.UtcNow,
                    Replace = true
                })
                .ExtendMetadata(facturXMetadata)
                .Save(outputPath);

            return File.ReadAllBytes(outputPath);
        }
        finally
        {
            TryDeleteFile(tempPath);
            TryDeleteFile(outputPath);
            TryDeleteFile(xmlTempPath);
        }
    }

    /// <summary>
    /// Construit le fragment XMP à injecter via ExtendMetadata.
    /// Ce fragment est ajouté dans un rdf:Description existant par QuestPDF.
    /// Il contient les propriétés Factur-X (fx:) et leur déclaration pdfaExtension:schemas.
    /// </summary>
    private static string BuildFacturXXmpExtension(FacturXRenderRequest request)
    {
        string conformanceLevel = request.Profile switch
        {
            FacturXProfile.Minimum => "MINIMUM",
            FacturXProfile.BasicWl => "BASIC WL",
            FacturXProfile.Basic => "BASIC",
            FacturXProfile.Extended => "EXTENDED",
            _ => "EN 16931"
        };

        string embeddedFileName = FacturXPdfEmbedder.Escape(request.EmbeddedFileName);

        return $"""
            <rdf:Description xmlns:pdfaExtension="http://www.aiim.org/pdfa/ns/extension/" xmlns:pdfaSchema="http://www.aiim.org/pdfa/ns/schema#" xmlns:pdfaProperty="http://www.aiim.org/pdfa/ns/property#" rdf:about="">
            <pdfaExtension:schemas>
              <rdf:Bag xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#">
                <rdf:li rdf:parseType="Resource">
                  <pdfaSchema:schema>Factur-X PDFA Extension Schema</pdfaSchema:schema>
                  <pdfaSchema:namespaceURI>urn:factur-x:pdfa:CrossIndustryDocument:invoice:1p0#</pdfaSchema:namespaceURI>
                  <pdfaSchema:prefix>fx</pdfaSchema:prefix>
                  <pdfaSchema:property>
                    <rdf:Seq>
                      <rdf:li rdf:parseType="Resource">
                        <pdfaProperty:name>DocumentFileName</pdfaProperty:name>
                        <pdfaProperty:valueType>Text</pdfaProperty:valueType>
                        <pdfaProperty:category>external</pdfaProperty:category>
                        <pdfaProperty:description>Name of the embedded XML invoice file</pdfaProperty:description>
                      </rdf:li>
                      <rdf:li rdf:parseType="Resource">
                        <pdfaProperty:name>DocumentType</pdfaProperty:name>
                        <pdfaProperty:valueType>Text</pdfaProperty:valueType>
                        <pdfaProperty:category>external</pdfaProperty:category>
                        <pdfaProperty:description>INVOICE</pdfaProperty:description>
                      </rdf:li>
                      <rdf:li rdf:parseType="Resource">
                        <pdfaProperty:name>Version</pdfaProperty:name>
                        <pdfaProperty:valueType>Text</pdfaProperty:valueType>
                        <pdfaProperty:category>external</pdfaProperty:category>
                        <pdfaProperty:description>Factur-X version</pdfaProperty:description>
                      </rdf:li>
                      <rdf:li rdf:parseType="Resource">
                        <pdfaProperty:name>ConformanceLevel</pdfaProperty:name>
                        <pdfaProperty:valueType>Text</pdfaProperty:valueType>
                        <pdfaProperty:category>external</pdfaProperty:category>
                        <pdfaProperty:description>Factur-X conformance level</pdfaProperty:description>
                      </rdf:li>
                    </rdf:Seq>
                  </pdfaSchema:property>
                </rdf:li>
              </rdf:Bag>
            </pdfaExtension:schemas>
            </rdf:Description>
            <rdf:Description xmlns:fx="urn:factur-x:pdfa:CrossIndustryDocument:invoice:1p0#" rdf:about="">
                <fx:DocumentType xmlns:fx="urn:factur-x:pdfa:CrossIndustryDocument:invoice:1p0#">INVOICE</fx:DocumentType>
                <fx:DocumentFileName xmlns:fx="urn:factur-x:pdfa:CrossIndustryDocument:invoice:1p0#">{embeddedFileName}</fx:DocumentFileName>
                <fx:Version xmlns:fx="urn:factur-x:pdfa:CrossIndustryDocument:invoice:1p0#">1.0</fx:Version>
                <fx:ConformanceLevel xmlns:fx="urn:factur-x:pdfa:CrossIndustryDocument:invoice:1p0#">{conformanceLevel}</fx:ConformanceLevel>
            </rdf:Description>
            """;
    }

    private static void TryDeleteFile(string path)
    {
        try { File.Delete(path); } catch { /* Nettoyage best-effort */ }
    }

    private static void ComposeHeader(IContainer container, FacturXRenderRequest request)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().Text("FACTURE").FontSize(22).Bold();
                col.Item().Text($"N° {request.InvoiceNumber}").FontSize(12).SemiBold();
                col.Item().Text($"Date : {request.IssueDate.ToString("dd/MM/yyyy", CultureInfo.GetCultureInfo("fr-FR"))}");
            });

            row.RelativeItem().AlignRight().Column(col =>
            {
                col.Item().Text(request.SellerName).SemiBold();
                col.Item().PaddingTop(10).Text(request.BuyerName).Italic();
            });
        });
    }

    private static void ComposeBasicContent(IContainer container, FacturXRenderRequest request)
    {
        container.PaddingVertical(20).Column(col =>
        {
            col.Item().LineHorizontal(1);
            col.Item().PaddingVertical(10).Row(row =>
            {
                row.RelativeItem().Text($"Montant HT : {FormatAmount(request.TotalWithoutVat, request.CurrencyCode)}");
                row.RelativeItem().Text($"TVA : {FormatAmount(request.TotalVat, request.CurrencyCode)}");
                row.RelativeItem().Text($"TTC : {FormatAmount(request.TotalWithVat, request.CurrencyCode)}").Bold();
            });
        });
    }

    private static void ComposeDetailedContent(IContainer container, FacturXRenderRequest request)
    {
        container.PaddingVertical(10).Column(col =>
        {
            // Parties
            col.Item().PaddingBottom(15).Row(row =>
            {
                row.RelativeItem().Border(1).Padding(8).Column(sellerCol =>
                {
                    sellerCol.Item().Text("VENDEUR").FontSize(9).Bold();
                    sellerCol.Item().Text(request.SellerName).SemiBold();
                    if (!string.IsNullOrWhiteSpace(request.SellerAddress))
                    {
                        sellerCol.Item().Text(request.SellerAddress).FontSize(9);
                    }

                    if (!string.IsNullOrWhiteSpace(request.SellerVatIdentifier))
                    {
                        sellerCol.Item().Text($"TVA : {request.SellerVatIdentifier}").FontSize(9);
                    }

                    if (!string.IsNullOrWhiteSpace(request.SellerLegalRegistrationId))
                    {
                        sellerCol.Item().Text($"SIREN : {request.SellerLegalRegistrationId}").FontSize(9);
                    }
                });

                row.ConstantItem(20);

                row.RelativeItem().Border(1).Padding(8).Column(buyerCol =>
                {
                    buyerCol.Item().Text("ACHETEUR").FontSize(9).Bold();
                    buyerCol.Item().Text(request.BuyerName).SemiBold();
                    if (!string.IsNullOrWhiteSpace(request.BuyerAddress))
                    {
                        buyerCol.Item().Text(request.BuyerAddress).FontSize(9);
                    }

                    if (!string.IsNullOrWhiteSpace(request.BuyerVatIdentifier))
                    {
                        buyerCol.Item().Text($"TVA : {request.BuyerVatIdentifier}").FontSize(9);
                    }

                    if (!string.IsNullOrWhiteSpace(request.BuyerLegalRegistrationId))
                    {
                        buyerCol.Item().Text($"SIREN : {request.BuyerLegalRegistrationId}").FontSize(9);
                    }
                });
            });

            // Tableau des lignes
            col.Item().PaddingBottom(10).Text("DÉTAIL DES PRESTATIONS").FontSize(11).Bold();

            col.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(35);
                    columns.RelativeColumn(3);
                    columns.ConstantColumn(50);
                    columns.ConstantColumn(50);
                    columns.ConstantColumn(70);
                    columns.ConstantColumn(45);
                    columns.ConstantColumn(75);
                });

                table.Header(header =>
                {
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Réf").FontSize(9).Bold();
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Description").FontSize(9).Bold();
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text("Qté").FontSize(9).Bold();
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Unité").FontSize(9).Bold();
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text("PU HT").FontSize(9).Bold();
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text("TVA %").FontSize(9).Bold();
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text("Montant HT").FontSize(9).Bold();
                });

                foreach (RenderLineItem line in request.Lines!)
                {
                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(line.Id).FontSize(9);
                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(line.Description).FontSize(9);
                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignRight().Text(line.Quantity.ToString("G", CultureInfo.GetCultureInfo("fr-FR"))).FontSize(9);
                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(line.UnitCode).FontSize(9);
                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignRight().Text(FormatAmount(line.UnitNetPrice, request.CurrencyCode)).FontSize(9);
                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignRight().Text($"{line.VatRate:G}%").FontSize(9);
                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignRight().Text(FormatAmount(line.NetAmount, request.CurrencyCode)).FontSize(9);
                }
            });

            col.Item().PaddingTop(15);

            // Ventilation TVA
            if (request.VatBreakdowns is { Count: > 0 })
            {
                col.Item().PaddingBottom(5).Text("VENTILATION TVA").FontSize(10).Bold();
                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Catégorie").FontSize(9).Bold();
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text("Taux").FontSize(9).Bold();
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text("Base HT").FontSize(9).Bold();
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text("Montant TVA").FontSize(9).Bold();
                    });

                    foreach (RenderVatBreakdown vat in request.VatBreakdowns)
                    {
                        table.Cell().Padding(4).Text(vat.Category).FontSize(9);
                        table.Cell().Padding(4).AlignRight().Text($"{vat.Rate:G}%").FontSize(9);
                        table.Cell().Padding(4).AlignRight().Text(FormatAmount(vat.BasisAmount, request.CurrencyCode)).FontSize(9);
                        table.Cell().Padding(4).AlignRight().Text(FormatAmount(vat.TaxAmount, request.CurrencyCode)).FontSize(9);
                    }
                });

                col.Item().PaddingTop(10);
            }

            // Totaux
            decimal amountDue = request.AmountDue ?? request.TotalWithVat;
            col.Item().AlignRight().Width(250).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn();
                    columns.ConstantColumn(100);
                });

                table.Cell().Padding(4).Text("Total HT").FontSize(10);
                table.Cell().Padding(4).AlignRight().Text(FormatAmount(request.TotalWithoutVat, request.CurrencyCode)).FontSize(10);

                table.Cell().Padding(4).Text("Total TVA").FontSize(10);
                table.Cell().Padding(4).AlignRight().Text(FormatAmount(request.TotalVat, request.CurrencyCode)).FontSize(10);

                table.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Total TTC").FontSize(11).Bold();
                table.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text(FormatAmount(request.TotalWithVat, request.CurrencyCode)).FontSize(11).Bold();

                table.Cell().Padding(4).Text("Net à payer").FontSize(10).SemiBold();
                table.Cell().Padding(4).AlignRight().Text(FormatAmount(amountDue, request.CurrencyCode)).FontSize(10).SemiBold();
            });

            // Modalités de paiement
            if (request.PaymentDueDate.HasValue || !string.IsNullOrWhiteSpace(request.PaymentIban))
            {
                col.Item().PaddingTop(15).Column(payCol =>
                {
                    payCol.Item().Text("MODALITÉS DE PAIEMENT").FontSize(10).Bold();
                    if (request.PaymentDueDate.HasValue)
                    {
                        payCol.Item().Text($"Échéance : {request.PaymentDueDate.Value.ToString("dd/MM/yyyy", CultureInfo.GetCultureInfo("fr-FR"))}").FontSize(9);
                    }

                    if (!string.IsNullOrWhiteSpace(request.PaymentIban))
                    {
                        payCol.Item().Text($"IBAN : {request.PaymentIban}").FontSize(9);
                    }
                });
            }
        });
    }

    private static void ComposeFooter(IContainer container)
    {
        container.AlignCenter().Text(text =>
        {
            text.DefaultTextStyle(x => x.FontSize(8).FontColor(Colors.Grey.Medium));
            text.Span("Factur-X EN16931 — facture électronique conforme à la réforme française | Page ");
            text.CurrentPageNumber();
            text.Span(" / ");
            text.TotalPages();
        });
    }

    private static string FormatAmount(decimal amount, string currency) =>
        string.Format(CultureInfo.GetCultureInfo("fr-FR"), "{0:N2} {1}", amount, currency);
}
