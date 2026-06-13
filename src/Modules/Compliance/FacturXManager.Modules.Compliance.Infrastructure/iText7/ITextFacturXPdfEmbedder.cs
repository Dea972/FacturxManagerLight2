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
using Microsoft.Extensions.Logging;

namespace FacturXManager.Modules.Compliance.Infrastructure.iText7;

/// <summary>
/// Implémentation de <see cref="IFacturXPdfEmbedder"/> basée sur iText 8.
/// Attache le XML CII (factur-x.xml, AFRelationship=Data) et injecte les métadonnées XMP Factur-X
/// entièrement en mémoire, sans fichier temporaire sur disque.
/// </summary>
internal sealed class ITextFacturXPdfEmbedder(ILogger<ITextFacturXPdfEmbedder> logger) : IFacturXPdfEmbedder
{
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
                "Début de l'embedding Factur-X (iText7) pour la facture {InvoiceNumber}.",
                request.InvoiceNumber);

            byte[] xmlBytes = Encoding.UTF8.GetBytes(request.CrossIndustryInvoiceXml);
            byte[] result = EmbedInMemory(visualPdfBytes, xmlBytes, request);

            logger.LogInformation(
                "Embedding Factur-X (iText7) terminé pour la facture {InvoiceNumber}. Taille PDF : {Size} octets.",
                request.InvoiceNumber,
                result.Length);

            return Task.FromResult(Result.Success(result));
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Échec de l'embedding Factur-X (iText7) pour la facture {InvoiceNumber}.",
                request.InvoiceNumber);
            return Task.FromResult(Result.Failure<byte[]>(InvoiceErrors.PdfEmbedFailed));
        }
    }

    private static byte[] EmbedInMemory(byte[] visualPdfBytes, byte[] xmlBytes, FacturXRenderRequest request)
    {
        using var inputStream = new MemoryStream(visualPdfBytes);
        using var outputStream = new MemoryStream();

        using (var pdfDoc = new PdfDocument(new PdfReader(inputStream), new PdfWriter(outputStream)))
        {
            AttachXmlFile(pdfDoc, xmlBytes, request.EmbeddedFileName);
            ExtendXmpMetadata(pdfDoc, request);
        }

        return outputStream.ToArray();
    }

    private static void AttachXmlFile(PdfDocument pdfDoc, byte[] xmlBytes, string fileName)
    {
        PdfDictionary fileParams = new PdfDictionary();
        fileParams.Put(PdfName.ModDate, new PdfDate(DateTime.UtcNow).GetPdfObject());
        fileParams.Put(PdfName.CreationDate, new PdfDate(DateTime.UtcNow).GetPdfObject());
        fileParams.Put(PdfName.Size, new PdfNumber(xmlBytes.Length));

        PdfFileSpec fileSpec = PdfFileSpec.CreateEmbeddedFileSpec(
            pdfDoc,
            xmlBytes,
            "Factur-X Invoice Data",
            fileName,
            new PdfName("application/xml"),
            fileParams,
            PdfName.Data);

        PdfDictionary catalog = pdfDoc.GetCatalog().GetPdfObject();

        // Add to /AF array (required for PDF/A-3 associated files)
        PdfArray af = catalog.GetAsArray(new PdfName("AF"));
        if (af is null)
        {
            af = new PdfArray();
            catalog.Put(new PdfName("AF"), af);
        }
        af.Add(fileSpec.GetPdfObject());

        // Add to EmbeddedFiles name tree
        PdfDictionary names = catalog.GetAsDictionary(PdfName.Names);
        if (names is null)
        {
            names = new PdfDictionary();
            catalog.Put(PdfName.Names, names);
        }

        PdfDictionary embeddedFiles = names.GetAsDictionary(PdfName.EmbeddedFiles);
        if (embeddedFiles is null)
        {
            embeddedFiles = new PdfDictionary();
            names.Put(PdfName.EmbeddedFiles, embeddedFiles);
        }

        PdfArray namesArray = embeddedFiles.GetAsArray(PdfName.Names);
        if (namesArray is null)
        {
            namesArray = new PdfArray();
            embeddedFiles.Put(PdfName.Names, namesArray);
        }

        namesArray.Add(new PdfString(fileName));
        namesArray.Add(fileSpec.GetPdfObject());
    }

    private static void ExtendXmpMetadata(PdfDocument pdfDoc, FacturXRenderRequest request)
    {
        byte[]? existingXmp = pdfDoc.GetXmpMetadata();
        string xmp = existingXmp is { Length: > 0 }
            ? Encoding.UTF8.GetString(existingXmp)
            : BuildMinimalXmpWrapper();

        string extension = BuildXmpExtension(request);

        // Inject Factur-X rdf:Description blocks before the closing </rdf:RDF> tag
        const string rdfClose = "</rdf:RDF>";
        int insertIdx = xmp.LastIndexOf(rdfClose, StringComparison.Ordinal);
        string augmented = insertIdx >= 0
            ? xmp.Insert(insertIdx, extension)
            : xmp + extension;

        pdfDoc.SetXmpMetadata(Encoding.UTF8.GetBytes(augmented));
    }

    private static string BuildMinimalXmpWrapper() =>
        """
        <?xpacket begin="" id="W5M0MpCehiHzreSzNTczkc9d"?>
        <x:xmpmeta xmlns:x="adobe:ns:meta/">
          <rdf:RDF xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#">
          </rdf:RDF>
        </x:xmpmeta>
        <?xpacket end="w"?>
        """;

    private static string BuildXmpExtension(FacturXRenderRequest request)
    {
        string conformanceLevel = MapConformanceLevel(request.Profile);
        string embeddedFileName = FacturXPdfEmbedderHelper.Escape(request.EmbeddedFileName);

        return $"""

            <rdf:Description xmlns:pdfaExtension="http://www.aiim.org/pdfa/ns/extension/" xmlns:pdfaSchema="http://www.aiim.org/pdfa/ns/schema#" xmlns:pdfaProperty="http://www.aiim.org/pdfa/ns/property#" rdf:about="">
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
            <rdf:Description xmlns:fx="urn:factur-x:pdfa:CrossIndustryDocument:invoice:1p0#" rdf:about="">
              <fx:DocumentType>INVOICE</fx:DocumentType>
              <fx:DocumentFileName>{embeddedFileName}</fx:DocumentFileName>
              <fx:Version>1.0</fx:Version>
              <fx:ConformanceLevel>{conformanceLevel}</fx:ConformanceLevel>
            </rdf:Description>

            """;
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
