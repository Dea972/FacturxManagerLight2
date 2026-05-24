using System.Globalization;
using System.Text;
using FacturXManager.Common.Domain;
using FacturXManager.Modules.Compliance.Application.Abstractions;
using FacturXManager.Modules.Compliance.Domain.Invoicing;
using Microsoft.Extensions.Logging;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.IO;

namespace FacturXManager.Modules.Compliance.Infrastructure.Invoicing;

/// <summary>
/// Implémentation PDF/A-3b basée sur PdfSharp 6.x.
/// Produit un PDF avec une mise en page lisible, le XML CII embarqué (factur-x.xml,
/// AFRelationship=Data, MIME application/xml) et les métadonnées XMP Factur-X obligatoires.
/// </summary>
internal sealed class FacturXPdfA3BRenderer : IFacturXPdfA3BRenderer
{
    private static readonly object FontInitLock = new();
    private static bool _fontsInitialized;

    private readonly ILogger<FacturXPdfA3BRenderer> _logger;

    public FacturXPdfA3BRenderer(ILogger<FacturXPdfA3BRenderer> logger)
    {
        _logger = logger;
        EnsureFontsInitialized();
    }

    public async Task<Result<byte[]>> RenderAsync(
        FacturXRenderRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var document = new PdfDocument();
            document.Options.FlateEncodeMode = PdfFlateEncodeMode.BestCompression;
            document.Options.NoCompression = false;
            document.Options.CompressContentStreams = true;

            ConfigureDocumentInfo(document, request);
            DrawInvoice(document, request);

            byte[] xmlBytes = Encoding.UTF8.GetBytes(request.CrossIndustryInvoiceXml);
            AttachFacturXXml(document, request.EmbeddedFileName, xmlBytes);
            AttachXmpMetadata(document, request);

            using var stream = new MemoryStream();
            await document.SaveAsync(stream, closeStream: false).ConfigureAwait(false);

            return Result.Success(stream.ToArray());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Échec du rendu PDF/A-3b Factur-X pour la facture {InvoiceNumber}.", request.InvoiceNumber);
            return Result.Failure<byte[]>(InvoiceErrors.PdfRendererNotConfigured);
        }
    }

    private static void EnsureFontsInitialized()
    {
        if (_fontsInitialized)
        {
            return;
        }

        lock (FontInitLock)
        {
            if (_fontsInitialized)
            {
                return;
            }

            GlobalFontSettings.UseWindowsFontsUnderWindows = true;
            GlobalFontSettings.UseWindowsFontsUnderWsl2 = true;

            GlobalFontSettings.FontResolver ??= new FailsafeFontResolver();

            _fontsInitialized = true;
        }
    }

    private static void ConfigureDocumentInfo(PdfDocument document, FacturXRenderRequest request)
    {
        document.Info.Title = $"Facture {request.InvoiceNumber}";
        document.Info.Author = request.SellerName;
        document.Info.Subject = "Facture Factur-X EN16931";
        document.Info.Creator = "FacturXManager";
        document.Info.Keywords = "Factur-X, EN16931, invoice";
        document.Info.CreationDate = DateTime.UtcNow;
    }

    private static void DrawInvoice(PdfDocument document, FacturXRenderRequest request)
    {
        PdfPage page = document.AddPage();
        page.Size = PdfSharp.PageSize.A4;

        using var gfx = XGraphics.FromPdfPage(page);

        var titleFont = new XFont("Arial", 20, XFontStyleEx.Bold);
        var headerFont = new XFont("Arial", 11, XFontStyleEx.Bold);
        var bodyFont = new XFont("Arial", 10, XFontStyleEx.Regular);
        var footerFont = new XFont("Arial", 8, XFontStyleEx.Italic);

        const double margin = 50;
        double width = page.Width.Point - margin * 2;
        double y = margin;

        // En-tête
        gfx.DrawString("FACTURE", titleFont, XBrushes.Black, new XRect(margin, y, width, 28), XStringFormats.TopLeft);
        y += 32;

        gfx.DrawString($"N° : {request.InvoiceNumber}", bodyFont, XBrushes.Black, new XPoint(margin, y));
        gfx.DrawString(
            $"Date d'émission : {request.IssueDate.ToString("dd/MM/yyyy", CultureInfo.GetCultureInfo("fr-FR"))}",
            bodyFont,
            XBrushes.Black,
            new XPoint(margin + 250, y));
        y += 16;
        gfx.DrawString($"Devise : {request.CurrencyCode}", bodyFont, XBrushes.Black, new XPoint(margin, y));
        y += 24;

        // Vendeur / Acheteur
        gfx.DrawString("Vendeur", headerFont, XBrushes.Black, new XPoint(margin, y));
        gfx.DrawString("Acheteur", headerFont, XBrushes.Black, new XPoint(margin + 280, y));
        y += 16;

        gfx.DrawString(request.SellerName, bodyFont, XBrushes.Black, new XPoint(margin, y));
        gfx.DrawString(request.BuyerName, bodyFont, XBrushes.Black, new XPoint(margin + 280, y));
        y += 40;

        // Séparateur
        gfx.DrawLine(XPens.Black, margin, y, margin + width, y);
        y += 16;

        // Totaux
        gfx.DrawString("Totaux", headerFont, XBrushes.Black, new XPoint(margin, y));
        y += 18;

        DrawTotalRow(gfx, bodyFont, margin, width, y, "Montant HT", request.TotalWithoutVat, request.CurrencyCode);
        y += 16;
        DrawTotalRow(gfx, bodyFont, margin, width, y, "TVA", request.TotalVat, request.CurrencyCode);
        y += 16;
        DrawTotalRow(gfx, headerFont, margin, width, y, "Montant TTC", request.TotalWithVat, request.CurrencyCode);

        // Pied de page
        double footerY = page.Height.Point - margin;
        gfx.DrawString(
            "Factur-X EN16931 — facture électronique conforme à la réforme française.",
            footerFont,
            XBrushes.Gray,
            new XRect(margin, footerY - 12, width, 12),
            XStringFormats.TopLeft);
    }

    private static void DrawTotalRow(XGraphics gfx, XFont font, double margin, double width, double y, string label, decimal amount, string currency)
    {
        gfx.DrawString(label, font, XBrushes.Black, new XPoint(margin, y));
        string formatted = string.Format(
            CultureInfo.GetCultureInfo("fr-FR"),
            "{0:N2} {1}",
            amount,
            currency);
        gfx.DrawString(formatted, font, XBrushes.Black, new XRect(margin, y, width, 14), XStringFormats.TopRight);
    }

    private static void AttachFacturXXml(PdfDocument document, string fileName, byte[] xmlBytes)
    {
        // Stream contenant le fichier XML embarqué.
        var embeddedStream = new PdfDictionary(document);
        embeddedStream.Elements.SetName("/Type", "/EmbeddedFile");
        embeddedStream.Elements.SetString("/Subtype", "application/xml");
        embeddedStream.CreateStream(xmlBytes);

        var paramsDict = new PdfDictionary(document);
        paramsDict.Elements.SetInteger("/Size", xmlBytes.Length);
        paramsDict.Elements.SetDateTime("/ModDate", DateTime.UtcNow);
        embeddedStream.Elements["/Params"] = paramsDict;

        document.Internals.AddObject(embeddedStream);

        var efDict = new PdfDictionary(document);
        efDict.Elements["/F"] = embeddedStream.Reference;
        efDict.Elements["/UF"] = embeddedStream.Reference;

        var fileSpec = new PdfDictionary(document);
        fileSpec.Elements.SetName("/Type", "/Filespec");
        fileSpec.Elements.SetString("/F", fileName);
        fileSpec.Elements.SetString("/UF", fileName);
        fileSpec.Elements.SetString("/Desc", "Factur-X invoice data");
        fileSpec.Elements.SetName("/AFRelationship", "/Data");
        fileSpec.Elements["/EF"] = efDict;
        document.Internals.AddObject(fileSpec);

        // Name tree /Names /EmbeddedFiles
        PdfCatalog catalog = document.Internals.Catalog;
        PdfDictionary namesDict = catalog.Elements.GetDictionary("/Names") ?? new PdfDictionary(document);
        var embeddedFilesDict = new PdfDictionary(document);
        var namesArray = new PdfArray(document);
        namesArray.Elements.Add(new PdfString(fileName));
        namesArray.Elements.Add(fileSpec.Reference!);
        embeddedFilesDict.Elements["/Names"] = namesArray;
        namesDict.Elements["/EmbeddedFiles"] = embeddedFilesDict;
        catalog.Elements["/Names"] = namesDict;

        // Tableau /AF sur le catalogue (associated files PDF/A-3).
        var afArray = new PdfArray(document);
        afArray.Elements.Add(fileSpec.Reference!);
        catalog.Elements["/AF"] = afArray;
    }

    private static void AttachXmpMetadata(PdfDocument document, FacturXRenderRequest request)
    {
        string conformanceLevel = request.Profile switch
        {
            FacturXProfile.Minimum => "MINIMUM",
            FacturXProfile.BasicWl => "BASIC WL",
            FacturXProfile.Basic => "BASIC",
            FacturXProfile.Extended => "EXTENDED",
            _ => "EN 16931"
        };

        string xmp = BuildXmpPacket(request, conformanceLevel);
        byte[] xmpBytes = Encoding.UTF8.GetBytes(xmp);

        var metadataDict = new PdfDictionary(document);
        metadataDict.Elements.SetName("/Type", "/Metadata");
        metadataDict.Elements.SetName("/Subtype", "/XML");
        metadataDict.CreateStream(xmpBytes);

        document.Internals.AddObject(metadataDict);
        document.Internals.Catalog.Elements["/Metadata"] = metadataDict.Reference;
    }

    private static string BuildXmpPacket(FacturXRenderRequest request, string conformanceLevel)
    {
        string creationDate = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
        return $"""
            <?xpacket begin="﻿" id="W5M0MpCehiHzreSzNTczkc9d"?>
            <x:xmpmeta xmlns:x="adobe:ns:meta/" x:xmptk="FacturXManager">
              <rdf:RDF xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#">
                <rdf:Description rdf:about=""
                    xmlns:pdfaid="http://www.aiim.org/pdfa/ns/id/">
                  <pdfaid:part>3</pdfaid:part>
                  <pdfaid:conformance>B</pdfaid:conformance>
                </rdf:Description>
                <rdf:Description rdf:about=""
                    xmlns:dc="http://purl.org/dc/elements/1.1/">
                  <dc:title><rdf:Alt><rdf:li xml:lang="x-default">Facture {Escape(request.InvoiceNumber)}</rdf:li></rdf:Alt></dc:title>
                  <dc:creator><rdf:Seq><rdf:li>{Escape(request.SellerName)}</rdf:li></rdf:Seq></dc:creator>
                </rdf:Description>
                <rdf:Description rdf:about=""
                    xmlns:xmp="http://ns.adobe.com/xap/1.0/">
                  <xmp:CreatorTool>FacturXManager</xmp:CreatorTool>
                  <xmp:CreateDate>{creationDate}</xmp:CreateDate>
                  <xmp:ModifyDate>{creationDate}</xmp:ModifyDate>
                </rdf:Description>
                <rdf:Description rdf:about=""
                    xmlns:pdfaExtension="http://www.aiim.org/pdfa/ns/extension/"
                    xmlns:pdfaSchema="http://www.aiim.org/pdfa/ns/schema#"
                    xmlns:pdfaProperty="http://www.aiim.org/pdfa/ns/property#">
                  <pdfaExtension:schemas>
                    <rdf:Bag>
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
                <rdf:Description rdf:about=""
                    xmlns:fx="urn:factur-x:pdfa:CrossIndustryDocument:invoice:1p0#">
                  <fx:DocumentType>INVOICE</fx:DocumentType>
                  <fx:DocumentFileName>{Escape(request.EmbeddedFileName)}</fx:DocumentFileName>
                  <fx:Version>1.0</fx:Version>
                  <fx:ConformanceLevel>{conformanceLevel}</fx:ConformanceLevel>
                </rdf:Description>
              </rdf:RDF>
            </x:xmpmeta>
            <?xpacket end="w"?>
            """;
    }

    private static string Escape(string value) =>
        value
            .Replace("&", "&amp;", StringComparison.Ordinal)
            .Replace("<", "&lt;", StringComparison.Ordinal)
            .Replace(">", "&gt;", StringComparison.Ordinal);

    /// <summary>
    /// Résolveur de polices : lit les polices depuis le dossier système Windows.
    /// Utilisé comme fallback quand PdfSharp ne trouve pas la police nativement.
    /// </summary>
    private sealed class FailsafeFontResolver : IFontResolver
    {
        private static readonly string FontsDirectory =
            Environment.GetFolderPath(Environment.SpecialFolder.Fonts);

        public byte[]? GetFont(string faceName)
        {
            string path = Path.Combine(FontsDirectory, faceName);
            return File.Exists(path) ? File.ReadAllBytes(path) : null;
        }

        public FontResolverInfo? ResolveTypeface(string familyName, bool bold, bool italic)
        {
            string fileName = (familyName.ToUpperInvariant(), bold, italic) switch
            {
                ("ARIAL", false, false) => "arial.ttf",
                ("ARIAL", true, false) => "arialbd.ttf",
                ("ARIAL", false, true) => "ariali.ttf",
                ("ARIAL", true, true) => "arialbi.ttf",
                _ => null!
            };

            return fileName is not null ? new FontResolverInfo(fileName) : null;
        }
    }
}
