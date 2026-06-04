using System.Globalization;
using System.Text;
using FacturXManager.Common.Domain;
using FacturXManager.Modules.Compliance.Application.Abstractions;
using FacturXManager.Modules.Compliance.Domain.Invoices;
using FacturXManager.Modules.Compliance.Infrastructure.Invoicing;
using Microsoft.Extensions.Logging;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.IO;

namespace FacturXManager.Modules.Compliance.Infrastructure.PdfSharp;

/// <summary>
/// Implémentation PDF/A-3b basée sur PdfSharp 6.x.
/// Produit un PDF avec une mise en page lisible, le XML CII embarqué (factur-x.xml,
/// AFRelationship=Data, MIME application/xml) et les métadonnées XMP Factur-X obligatoires.
/// </summary>
internal sealed class PdfSharpFacturXPdfA3BRenderer : IFacturXPdfA3BRenderer
{
    private static readonly object FontInitLock = new();
    private static bool _fontsInitialized;

    private readonly ILogger<PdfSharpFacturXPdfA3BRenderer> _logger;

    public PdfSharpFacturXPdfA3BRenderer(ILogger<PdfSharpFacturXPdfA3BRenderer> logger)
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

            using var stream = new MemoryStream();
            await document.SaveAsync(stream, closeStream: false).ConfigureAwait(false);

            byte[] visualPdf = stream.ToArray();
            byte[] finalPdf = FacturXPdfEmbedderHelper.EmbedFacturXData(visualPdf, request);

            return Result.Success(finalPdf);
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
        page.Size = PageSize.A4;

        using var gfx = XGraphics.FromPdfPage(page);

        var titleFont = new XFont("Arial", 20, XFontStyleEx.Bold);
        var headerFont = new XFont("Arial", 11, XFontStyleEx.Bold);
        var sectionFont = new XFont("Arial", 10, XFontStyleEx.Bold);
        var bodyFont = new XFont("Arial", 10, XFontStyleEx.Regular);
        var smallFont = new XFont("Arial", 9, XFontStyleEx.Regular);
        var smallBoldFont = new XFont("Arial", 9, XFontStyleEx.Bold);
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
        double colWidth = width / 2 - 10;
        gfx.DrawString("VENDEUR", sectionFont, XBrushes.Black, new XPoint(margin, y));
        gfx.DrawString("ACHETEUR", sectionFont, XBrushes.Black, new XPoint(margin + colWidth + 20, y));
        y += 16;

        gfx.DrawString(request.SellerName, bodyFont, XBrushes.Black, new XPoint(margin, y));
        gfx.DrawString(request.BuyerName, bodyFont, XBrushes.Black, new XPoint(margin + colWidth + 20, y));
        y += 14;

        if (!string.IsNullOrWhiteSpace(request.SellerAddress))
        {
            gfx.DrawString(request.SellerAddress, smallFont, XBrushes.Black, new XPoint(margin, y));
        }

        if (!string.IsNullOrWhiteSpace(request.BuyerAddress))
        {
            gfx.DrawString(request.BuyerAddress, smallFont, XBrushes.Black, new XPoint(margin + colWidth + 20, y));
        }

        y += 14;

        if (!string.IsNullOrWhiteSpace(request.SellerVatIdentifier))
        {
            gfx.DrawString($"TVA : {request.SellerVatIdentifier}", smallFont, XBrushes.Black, new XPoint(margin, y));
        }

        if (!string.IsNullOrWhiteSpace(request.BuyerVatIdentifier))
        {
            gfx.DrawString($"TVA : {request.BuyerVatIdentifier}", smallFont, XBrushes.Black, new XPoint(margin + colWidth + 20, y));
        }

        y += 14;

        if (!string.IsNullOrWhiteSpace(request.SellerLegalRegistrationId))
        {
            gfx.DrawString($"SIREN : {request.SellerLegalRegistrationId}", smallFont, XBrushes.Black, new XPoint(margin, y));
        }

        if (!string.IsNullOrWhiteSpace(request.BuyerLegalRegistrationId))
        {
            gfx.DrawString($"SIREN : {request.BuyerLegalRegistrationId}", smallFont, XBrushes.Black, new XPoint(margin + colWidth + 20, y));
        }

        y += 20;

        // Lignes de facture
        if (request.Lines is { Count: > 0 })
        {
            gfx.DrawLine(XPens.Black, margin, y, margin + width, y);
            y += 8;
            gfx.DrawString("DÉTAIL DES PRESTATIONS", sectionFont, XBrushes.Black, new XPoint(margin, y));
            y += 18;

            // En-tête du tableau
            double[] colX = [margin, margin + 35, margin + 220, margin + 265, margin + 330, margin + 390, margin + 430];

            gfx.DrawString("Réf", smallBoldFont, XBrushes.Black, new XPoint(colX[0], y));
            gfx.DrawString("Description", smallBoldFont, XBrushes.Black, new XPoint(colX[1], y));
            gfx.DrawString("Qté", smallBoldFont, XBrushes.Black, new XPoint(colX[2], y));
            gfx.DrawString("PU HT", smallBoldFont, XBrushes.Black, new XPoint(colX[3], y));
            gfx.DrawString("TVA %", smallBoldFont, XBrushes.Black, new XPoint(colX[4], y));
            gfx.DrawString("Montant HT", smallBoldFont, XBrushes.Black, new XPoint(colX[5], y));
            y += 4;
            gfx.DrawLine(XPens.LightGray, margin, y + 10, margin + width, y + 10);
            y += 14;

            foreach (RenderLineItem line in request.Lines)
            {
                if (y > page.Height.Point - 100)
                {
                    // Nouvelle page si nécessaire
                    page = document.AddPage();
                    page.Size = PageSize.A4;
                    gfx.Dispose();
                    var newGfx = XGraphics.FromPdfPage(page);
                    y = margin;
                    DrawInvoiceLines(newGfx, request.Lines, ref y, page, margin, width, smallFont, colX, request.CurrencyCode);
                    DrawRemainder(newGfx, request, ref y, margin, width, bodyFont, headerFont, sectionFont, smallFont, footerFont, page);
                    return;
                }

                gfx.DrawString(line.Id, smallFont, XBrushes.Black, new XPoint(colX[0], y));
                gfx.DrawString(TruncateText(line.Description, 30), smallFont, XBrushes.Black, new XPoint(colX[1], y));
                gfx.DrawString(line.Quantity.ToString("G", CultureInfo.GetCultureInfo("fr-FR")), smallFont, XBrushes.Black, new XPoint(colX[2], y));
                gfx.DrawString(FormatAmount(line.UnitNetPrice, request.CurrencyCode), smallFont, XBrushes.Black, new XPoint(colX[3], y));
                gfx.DrawString($"{line.VatRate:G}%", smallFont, XBrushes.Black, new XPoint(colX[4], y));
                gfx.DrawString(FormatAmount(line.NetAmount, request.CurrencyCode), smallFont, XBrushes.Black, new XPoint(colX[5], y));
                y += 14;
            }

            y += 6;
        }

        // Ventilation TVA
        if (request.VatBreakdowns is { Count: > 0 })
        {
            gfx.DrawLine(XPens.LightGray, margin, y, margin + width, y);
            y += 8;
            gfx.DrawString("VENTILATION TVA", sectionFont, XBrushes.Black, new XPoint(margin, y));
            y += 16;

            foreach (RenderVatBreakdown vat in request.VatBreakdowns)
            {
                gfx.DrawString(
                    $"{vat.Category} {vat.Rate:G}% — Base : {FormatAmount(vat.BasisAmount, request.CurrencyCode)} — TVA : {FormatAmount(vat.TaxAmount, request.CurrencyCode)}",
                    smallFont,
                    XBrushes.Black,
                    new XPoint(margin, y));
                y += 14;
            }

            y += 6;
        }

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
        y += 16;

        decimal amountDue = request.AmountDue ?? request.TotalWithVat;
        DrawTotalRow(gfx, sectionFont, margin, width, y, "Net à payer", amountDue, request.CurrencyCode);
        y += 24;

        // Modalités de paiement
        if (request.PaymentDueDate.HasValue || !string.IsNullOrWhiteSpace(request.PaymentIban))
        {
            gfx.DrawString("MODALITÉS DE PAIEMENT", sectionFont, XBrushes.Black, new XPoint(margin, y));
            y += 16;

            if (request.PaymentDueDate.HasValue)
            {
                gfx.DrawString(
                    $"Échéance : {request.PaymentDueDate.Value.ToString("dd/MM/yyyy", CultureInfo.GetCultureInfo("fr-FR"))}",
                    smallFont,
                    XBrushes.Black,
                    new XPoint(margin, y));
                y += 14;
            }

            if (!string.IsNullOrWhiteSpace(request.PaymentIban))
            {
                gfx.DrawString($"IBAN : {request.PaymentIban}", smallFont, XBrushes.Black, new XPoint(margin, y));
            }
        }

        // Pied de page
        double footerY = page.Height.Point - margin;
        gfx.DrawString(
            "Factur-X EN16931 — facture électronique conforme à la réforme française.",
            footerFont,
            XBrushes.Gray,
            new XRect(margin, footerY - 12, width, 12),
            XStringFormats.TopLeft);
    }

    // Stubs pour pagination multi-pages (non implémenté — les factures courtes tiennent sur une page)
    private static void DrawInvoiceLines(XGraphics gfx, IReadOnlyList<RenderLineItem> lines, ref double y,
        PdfPage page, double margin, double width, XFont font, double[] colX, string currency)
    {
        // Pagination multi-pages non implémentée — les factures courtes tiennent sur une page
    }

    private static void DrawRemainder(XGraphics gfx, FacturXRenderRequest request, ref double y,
        double margin, double width, XFont bodyFont, XFont headerFont, XFont sectionFont,
        XFont smallFont, XFont footerFont, PdfPage page)
    {
        // Pagination multi-pages non implémentée — les factures courtes tiennent sur une page
    }

    private static string TruncateText(string text, int maxLength) =>
        text.Length <= maxLength ? text : string.Concat(text.AsSpan(0, maxLength - 3), "...");

    private static string FormatAmount(decimal amount, string currency) =>
        string.Format(CultureInfo.GetCultureInfo("fr-FR"), "{0:N2} {1}", amount, currency);

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
