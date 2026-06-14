using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FacturXManager.Common.Domain;
using FacturXManager.Modules.Compliance.Application.Abstractions;
using FacturXManager.Modules.Compliance.Domain.Common;
using FacturXManager.Modules.Compliance.Domain.Invoices;
using FacturXManager.Modules.Compliance.Infrastructure.FacturX;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Filespec;
using iText.Kernel.XMP;
using iText.Kernel.XMP.Options;
using iText.Pdfa;
using Microsoft.Extensions.Logging;

namespace FacturXManager.Modules.Compliance.Infrastructure.iText7;

/// <summary>
/// Implémentation de <see cref="IFacturXPdfEmbedder"/> basée sur iText 9.
/// Attache le XML CII (factur-x.xml, AFRelationship=Data) et injecte les métadonnées XMP Factur-X
/// entièrement en mémoire, sans fichier temporaire sur disque.
/// </summary>
internal sealed class TextFacturXPdfEmbedder(ILogger<TextFacturXPdfEmbedder> logger) : IFacturXPdfEmbedder
{
    // Namespace officiel Factur-X 1.0 — ne pas confondre avec urn:ferd (ZUGFeRD legacy)
    private const string FacturXNamespace = "urn:factur-x:pdfa:CrossIndustryDocument:invoice:1p0#";
    private const string FacturXPrefix = "fx";

    public Task<Result<byte[]>> EmbedAsync(
        byte[] visualPdfBytes,
        FacturXRenderRequest request,
        CancellationToken cancellationToken = default)
    {
        if (visualPdfBytes is null || visualPdfBytes.Length == 0)
        {
            return Task.FromResult(Result.Failure<byte[]>(InvoiceErrors.InvalidPdf));
        }

        try
        {
            logger.LogInformation(
                "Début de l'embedding Factur-X (iText9) pour la facture {InvoiceNumber}.",
                request.InvoiceNumber);

            byte[] xmlBytes = Encoding.UTF8.GetBytes(request.CrossIndustryInvoiceXml);
            byte[] result = EmbedInMemory(visualPdfBytes, xmlBytes, request);

            logger.LogInformation(
                "Embedding Factur-X (iText9) terminé pour la facture {InvoiceNumber}. Taille PDF : {Size} octets.",
                request.InvoiceNumber,
                result.Length);

            return Task.FromResult(Result.Success(result));
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Échec de l'embedding Factur-X (iText9) pour la facture {InvoiceNumber}.",
                request.InvoiceNumber);
            return Task.FromResult(Result.Failure<byte[]>(InvoiceErrors.PdfEmbedFailed));
        }
    }

    private static byte[] EmbedInMemory(byte[] visualPdfBytes, byte[] xmlBytes, FacturXRenderRequest request)
    {
        using var inputStream = new MemoryStream(visualPdfBytes);
        using var outputStream = new MemoryStream();

        using var reader = new PdfReader(inputStream);
        using var writer = new PdfWriter(outputStream);

        // PdfDocument au lieu de PdfADocument — les PDFs clients ne sont pas PDF/A conformants
        using (var pdfDoc = new PdfDocument(reader, writer))
        {
            AttachXmlFile(pdfDoc, xmlBytes, request.EmbeddedFileName);
            ExtendXmpMetadata(pdfDoc, request);
        }

        return outputStream.ToArray();
    }

    private static void AttachXmlFile(PdfDocument pdfDoc, byte[] xmlBytes, string fileName)
    {
        var fileParams = new PdfDictionary();
        fileParams.Put(PdfName.ModDate, new PdfDate(DateTime.UtcNow).GetPdfObject());
        fileParams.Put(PdfName.CreationDate, new PdfDate(DateTime.UtcNow).GetPdfObject());
        fileParams.Put(PdfName.Size, new PdfNumber(xmlBytes.Length));

        var fileSpec = PdfFileSpec.CreateEmbeddedFileSpec(
            pdfDoc,
            xmlBytes,
            "Factur-X Invoice Data",
            fileName,
            new PdfName("text/xml"),   // ← était "application/xml", le validateur exige "text/xml"
            fileParams,
            PdfName.Data);

        // Rendre indirect AVANT l'ajout au /AF — sinon Chorus Pro ne peut pas résoudre la référence
        fileSpec.GetPdfObject().MakeIndirect(pdfDoc);

        // AddFileAttachment gère l'EmbeddedFiles name tree
        pdfDoc.AddFileAttachment(fileName, fileSpec);

        // /AF dans le catalogue avec référence indirecte
        PdfDictionary catalog = pdfDoc.GetCatalog().GetPdfObject();
        PdfArray af = catalog.GetAsArray(new PdfName("AF")) ?? new PdfArray();
        af.Add(fileSpec.GetPdfObject().GetIndirectReference());
        catalog.Put(new PdfName("AF"), af);
    }

    private static void ExtendXmpMetadata(PdfDocument pdfDoc, FacturXRenderRequest request)
    {
        XMPMeta xmp = pdfDoc.GetXmpMetadata(true);

        XMPMetaFactory.GetSchemaRegistry().RegisterNamespace(FacturXNamespace, FacturXPrefix);

        // Propriétés Factur-X principales
        xmp.SetProperty(FacturXNamespace, "DocumentType", "INVOICE");
        xmp.SetProperty(FacturXNamespace, "Version", "1.0");
        xmp.SetProperty(FacturXNamespace, "DocumentFileName", request.EmbeddedFileName);
        xmp.SetProperty(FacturXNamespace, "ConformanceLevel", MapConformanceLevel(request.Profile));
        // Déclaration PDF/A-3b — sans ça les validateurs appliquent les règles PDF/A-1
        xmp.SetProperty(XMPConst.NS_PDFA_ID, "part", "3");
        xmp.SetProperty(XMPConst.NS_PDFA_ID, "conformance", "B");
        // Extension schema PDF/A requise pour la conformité PDF/A-3
        var bagOptions = new PropertyOptions(PropertyOptions.ARRAY);
        xmp.SetProperty(XMPConst.NS_PDFA_EXTENSION, "schemas", null, bagOptions);

        string bagPath = "pdfaExtension:schemas";
        int itemIndex = xmp.CountArrayItems(XMPConst.NS_PDFA_EXTENSION, bagPath) + 1;
        string itemPath = $"{bagPath}[{itemIndex}]";

        var structOptions = new PropertyOptions(PropertyOptions.STRUCT);
        xmp.SetProperty(XMPConst.NS_PDFA_EXTENSION, itemPath, null, structOptions);
        xmp.SetStructField(XMPConst.NS_PDFA_EXTENSION, itemPath, XMPConst.NS_PDFA_SCHEMA, "schema",
            "Factur-X PDFA Extension Schema");
        xmp.SetStructField(XMPConst.NS_PDFA_EXTENSION, itemPath, XMPConst.NS_PDFA_SCHEMA, "namespaceURI",
            FacturXNamespace);
        xmp.SetStructField(XMPConst.NS_PDFA_EXTENSION, itemPath, XMPConst.NS_PDFA_SCHEMA, "prefix",
            FacturXPrefix);

        // Déclaration des 4 propriétés du schema fx
        string seqPath = itemPath + "/pdfaSchema:property";
        xmp.SetProperty(XMPConst.NS_PDFA_EXTENSION, seqPath, null,
            new PropertyOptions(PropertyOptions.ARRAY_ORDERED));

        AddXmpPropertyDeclaration(xmp, seqPath, 1, structOptions, "DocumentFileName",
            "Name of the embedded XML invoice file");
        AddXmpPropertyDeclaration(xmp, seqPath, 2, structOptions, "DocumentType",
            "INVOICE");
        AddXmpPropertyDeclaration(xmp, seqPath, 3, structOptions, "Version",
            "Factur-X version");
        AddXmpPropertyDeclaration(xmp, seqPath, 4, structOptions, "ConformanceLevel",
            "Factur-X conformance level");

        pdfDoc.SetXmpMetadata(xmp);
    }

    private static void AddXmpPropertyDeclaration(
        XMPMeta xmp, string seqPath, int index, PropertyOptions structOptions,
        string name, string description)
    {
        string path = $"{seqPath}[{index}]";
        xmp.SetProperty(XMPConst.NS_PDFA_EXTENSION, path, null, structOptions);
        xmp.SetStructField(XMPConst.NS_PDFA_EXTENSION, path, XMPConst.NS_PDFA_PROPERTY, "name", name);
        xmp.SetStructField(XMPConst.NS_PDFA_EXTENSION, path, XMPConst.NS_PDFA_PROPERTY, "valueType", "Text");
        xmp.SetStructField(XMPConst.NS_PDFA_EXTENSION, path, XMPConst.NS_PDFA_PROPERTY, "category", "external");
        xmp.SetStructField(XMPConst.NS_PDFA_EXTENSION, path, XMPConst.NS_PDFA_PROPERTY, "description", description);
    }

    private static string MapConformanceLevel(FacturXProfile profile) => profile switch
    {
        FacturXProfile.Minimum => "MINIMUM",
        FacturXProfile.BasicWl => "BASIC WL",
        FacturXProfile.Basic => "BASIC",
        FacturXProfile.Extended => "EXTENDED",
        _ => "EN 16931"
    };
}
