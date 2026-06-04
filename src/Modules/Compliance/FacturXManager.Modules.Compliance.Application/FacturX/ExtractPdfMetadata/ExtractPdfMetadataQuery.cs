using FacturXManager.Common.Application.Messaging;

namespace FacturXManager.Modules.Compliance.Application.FacturX.ExtractPdfMetadata;

/// <summary>
/// Types de métadonnées extractibles d'un PDF Factur-X.
/// </summary>
public enum PdfMetadataType
{
    /// <summary>XMP du catalogue PDF (pdfaid, fx:, Dublin Core...).</summary>
    Xmp,

    /// <summary>XML CII embarqué (factur-x.xml).</summary>
    EmbeddedXml,

    /// <summary>Archive ZIP contenant le PDF Factur-X, le XML CII et le XMP.</summary>
    Zip
}

public sealed record ExtractPdfMetadataQuery(
    byte[] PdfBytes,
    PdfMetadataType MetadataType) : IQuery<ExtractPdfMetadataResponse>;
