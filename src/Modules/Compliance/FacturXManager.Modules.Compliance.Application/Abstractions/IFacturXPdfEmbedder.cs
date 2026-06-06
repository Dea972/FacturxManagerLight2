using FacturXManager.Common.Domain;

namespace FacturXManager.Modules.Compliance.Application.Abstractions;

/// <summary>
/// Point d'extension permettant d'attacher des données de conformité Factur-X (XML CII + XMP)
/// à un document PDF visuel existant.
/// </summary>
public interface IFacturXPdfEmbedder
{
    /// <summary>
    /// Intègre le XML CII et les métadonnées Factur-X dans un PDF existant.
    /// </summary>
    /// <param name="visualPdfBytes">Les octets du document PDF visuel source.</param>
    /// <param name="request">Les informations Factur-X à embarquer.</param>
    /// <param name="cancellationToken">Le jeton d'annulation.</param>
    /// <returns>Les octets du fichier PDF/A-3b résultant.</returns>
    Task<Result<byte[]>> EmbedAsync(
        byte[] visualPdfBytes,
        FacturXRenderRequest request,
        CancellationToken cancellationToken = default);
}
