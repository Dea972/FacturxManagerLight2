namespace FacturXManager.Modules.Compliance.Application.Abstractions;

/// <summary>
/// Extrait les métadonnées d'un PDF Factur-X : XMP du catalogue et XML CII embarqué.
/// </summary>
public interface IFacturXMetadataExtractor
{
    /// <summary>
    /// Extrait le XMP brut (chaîne XML) du catalogue du PDF.
    /// Retourne une chaîne vide si le XMP est absent ou compressé.
    /// </summary>
    string ExtractXmp(byte[] pdfBytes);

    /// <summary>
    /// Extrait le contenu du fichier XML CII embarqué (factur-x.xml).
    /// Retourne une chaîne vide si aucun fichier XML n'est trouvé.
    /// </summary>
    string ExtractEmbeddedXml(byte[] pdfBytes);
}
