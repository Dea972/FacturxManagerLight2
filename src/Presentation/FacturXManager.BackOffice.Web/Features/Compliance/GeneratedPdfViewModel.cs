namespace FacturXManager.BackOffice.Web.Features.Compliance;

/// <summary>
/// PDF généré prêt à être déclenché en téléchargement dans le navigateur.
/// </summary>
public sealed record GeneratedPdfViewModel(string FileName, byte[] Content);
