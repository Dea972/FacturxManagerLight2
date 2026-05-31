using System.Text;
using FacturXManager.Modules.Compliance.Application.Abstractions;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.IO;

namespace FacturXManager.Modules.Compliance.Infrastructure.Invoicing;

/// <summary>
/// Extrait les métadonnées d'un PDF Factur-X via PdfSharp :
/// XMP du catalogue (/Metadata) et XML CII embarqué (/Names/EmbeddedFiles).
/// </summary>
internal sealed class FacturXMetadataExtractor : IFacturXMetadataExtractor
{
    public string ExtractXmp(byte[] pdfBytes)
    {
        try
        {
            using var stream = new MemoryStream(pdfBytes);
            using PdfDocument document = PdfReader.Open(stream, PdfDocumentOpenMode.Import);

            PdfCatalog catalog = document.Internals.Catalog;

            if (catalog.Elements["/Metadata"] is PdfReference metaRef
                && metaRef.Value is PdfDictionary meta
                && meta.Stream is not null
                && meta.Stream.Value.Length > 0)
            {
                string xmp = Encoding.UTF8.GetString(meta.Stream.Value);
                if (xmp.Contains("<?xpacket", StringComparison.Ordinal))
                {
                    return xmp.Trim();
                }
            }

            return TryExtractXmpFromRawBytes(pdfBytes);
        }
        catch
        {
            return TryExtractXmpFromRawBytes(pdfBytes);
        }
    }

    public string ExtractEmbeddedXml(byte[] pdfBytes)
    {
        try
        {
            using var stream = new MemoryStream(pdfBytes);
            using PdfDocument document = PdfReader.Open(stream, PdfDocumentOpenMode.Import);

            PdfCatalog catalog = document.Internals.Catalog;

            PdfDictionary? namesDict = catalog.Elements.GetDictionary("/Names");
            PdfDictionary? embeddedFiles = namesDict?.Elements.GetDictionary("/EmbeddedFiles");
            PdfArray? namesArray = embeddedFiles?.Elements.GetArray("/Names");

            if (namesArray is null)
            {
                return string.Empty;
            }

            // Structure : [string fileName, reference fileSpec, ...]
            for (int i = 0; i + 1 < namesArray.Elements.Count; i += 2)
            {
                string? name = namesArray.Elements[i] is PdfString pdfStr ? pdfStr.Value : null;

                if (name is null || !name.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                PdfDictionary? fileSpec = namesArray.Elements[i + 1] is PdfReference fileRef
                    ? fileRef.Value as PdfDictionary
                    : namesArray.Elements[i + 1] as PdfDictionary;

                PdfDictionary? ef = fileSpec?.Elements.GetDictionary("/EF");
                PdfDictionary? embeddedStream = ef?.Elements["/F"] is PdfReference efRef
                    ? efRef.Value as PdfDictionary
                    : null;

                if (embeddedStream?.Stream is null)
                {
                    continue;
                }

                return Encoding.UTF8.GetString(embeddedStream.Stream.Value).Trim();
            }

            return string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Cherche le xpacket XMP directement dans les octets bruts du PDF
    /// (fallback quand le stream /Metadata est compressé par PdfSharp).
    /// </summary>
    private static string TryExtractXmpFromRawBytes(byte[] pdfBytes)
    {
        string pdfString = Encoding.Latin1.GetString(pdfBytes);

        const string packetBegin = "<?xpacket begin=";
        const string packetEnd = "<?xpacket end=\"w\"?>";

        int startIdx = pdfString.IndexOf(packetBegin, StringComparison.Ordinal);
        if (startIdx < 0)
        {
            return string.Empty;
        }

        int endIdx = pdfString.IndexOf(packetEnd, startIdx, StringComparison.Ordinal);
        if (endIdx < 0)
        {
            return string.Empty;
        }

        return pdfString[startIdx..(endIdx + packetEnd.Length)].Trim();
    }
}
