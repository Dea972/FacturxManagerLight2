using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FacturXManager.Common.Domain;
using FacturXManager.Modules.Compliance.Application.Abstractions;
using FacturXManager.Modules.Compliance.Domain.Common;
using FacturXManager.Modules.Compliance.Domain.Invoices;
using FacturXManager.Modules.Compliance.Infrastructure.Invoicing;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;

namespace FacturXManager.Modules.Compliance.Infrastructure.FacturX;

/// <summary>
/// Implémentation de <see cref="IFacturXPdfEmbedder"/> basée sur QuestPDF <see cref="DocumentOperation"/>.
/// Attache le XML CII (factur-x.xml, AFRelationship=Data) et injecte les métadonnées XMP Factur-X
/// dans un PDF visuel existant fourni, sans patch binaire ni gestion manuelle des xref.
/// </summary>
internal sealed class QuestPdfFacturXEmbedder(ILogger<QuestPdfFacturXEmbedder> logger) : IFacturXPdfEmbedder
{
    public async Task<Result<byte[]>> EmbedAsync(
        byte[] visualPdfBytes,
        FacturXRenderRequest request,
        CancellationToken cancellationToken = default)
    {
        if (visualPdfBytes == null || visualPdfBytes.Length == 0)
        {
            return Result.Failure<byte[]>(InvoiceErrors.InvalidPdf);
        }

        string inputPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".pdf");
        string outputPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".pdf");
        string xmlTempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".xml");

        try
        {
            logger.LogInformation(
                "Début de l'embedding Factur-X (QuestPDF) pour la facture {InvoiceNumber}.",
                request.InvoiceNumber);

            await File.WriteAllBytesAsync(inputPath, visualPdfBytes, cancellationToken);
            await File.WriteAllBytesAsync(xmlTempPath, Encoding.UTF8.GetBytes(request.CrossIndustryInvoiceXml), cancellationToken);

            string xmpExtension = BuildXmpExtension(request);

            DocumentOperation
                .LoadFile(inputPath)
                .AddAttachment(new DocumentOperation.DocumentAttachment
                {
                    Key = "factur-x",
                    FilePath = xmlTempPath,
                    AttachmentName = request.EmbeddedFileName,
                    MimeType = "application/xml",
                    Description = "Factur-X Invoice Data",
                    Relationship = DocumentOperation.DocumentAttachmentRelationship.Data,
                    CreationDate = DateTime.UtcNow,
                    ModificationDate = DateTime.UtcNow,
                    Replace = true
                })
                .ExtendMetadata(xmpExtension)
                .Save(outputPath);

            byte[] result = await File.ReadAllBytesAsync(outputPath, cancellationToken);

            logger.LogInformation(
                "Embedding Factur-X (QuestPDF) terminé pour la facture {InvoiceNumber}. Taille PDF : {Size} octets.",
                request.InvoiceNumber,
                result.Length);

            return Result.Success(result);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Échec de l'embedding Factur-X (QuestPDF) pour la facture {InvoiceNumber}.",
                request.InvoiceNumber);
            return Result.Failure<byte[]>(InvoiceErrors.PdfEmbedFailed);
        }
        finally
        {
            TryDeleteFile(inputPath);
            TryDeleteFile(outputPath);
            TryDeleteFile(xmlTempPath);
        }
    }

    /// <summary>
    /// Construit le fragment XMP injecté via ExtendMetadata.
    /// Contient la déclaration du schéma d'extension PDF/A (pdfaExtension:schemas) et les
    /// propriétés Factur-X (fx:DocumentType, fx:DocumentFileName, fx:Version, fx:ConformanceLevel).
    /// </summary>
    private static string BuildXmpExtension(FacturXRenderRequest request)
    {
        string conformanceLevel = MapConformanceLevel(request.Profile);
        string embeddedFileName = FacturXPdfEmbedderHelper.Escape(request.EmbeddedFileName);

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

    private static void TryDeleteFile(string path)
    {
        try { File.Delete(path); } catch { /* Nettoyage best-effort */ }
    }
}
