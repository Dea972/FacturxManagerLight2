using FacturXManager.Common.Domain;
using FacturXManager.Modules.Compliance.Application.Abstractions;
using FacturXManager.Modules.Compliance.Domain.Invoicing;

namespace FacturXManager.Modules.Compliance.Infrastructure.Invoicing;

/// <summary>
/// Implémentation par défaut (non opérationnelle) du moteur de rendu PDF/A-3b.
/// </summary>
/// <remarks>
/// La fabrication d'un PDF/A-3b avec polices embarquées et fichier <c>factur-x.xml</c> attaché
/// nécessite une bibliothèque PDF dédiée (ex : PdfSharp 6.x pour la composition + conformité PDF/A,
/// ou s2industries.ZUGFeRD pour l'embarquement Factur-X). Pour activer la certification :
/// <list type="number">
///   <item>ajouter le package choisi dans <c>Directory.Packages.props</c> ;</item>
///   <item>implémenter <see cref="IFacturXPdfA3bRenderer"/> en produisant un PDF/A-3b qui :
///     génère la représentation visuelle de la facture, embarque
///     <see cref="FacturXRenderRequest.CrossIndustryInvoiceXml"/> sous le nom
///     <see cref="FacturXRenderRequest.EmbeddedFileName"/> (relation AFRelationship=Data),
///     et passe la validation VeraPDF avant tout dépôt ;</item>
///   <item>remplacer l'enregistrement DI de cette classe par la nouvelle implémentation.</item>
/// </list>
/// </remarks>
internal sealed class UnconfiguredFacturXPdfA3bRenderer : IFacturXPdfA3bRenderer
{
    public Task<Result<byte[]>> RenderAsync(
        FacturXRenderRequest request,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(Result.Failure<byte[]>(InvoiceErrors.PdfRendererNotConfigured));
}
