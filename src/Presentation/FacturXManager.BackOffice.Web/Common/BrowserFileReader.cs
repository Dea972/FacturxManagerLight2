using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components.Forms;

namespace FacturXManager.BackOffice.Web.Common;

/// <summary>
/// Helpers de lecture des fichiers uploadés côté Blazor.
/// </summary>
public static class BrowserFileReader
{
    /// <summary>Taille maximale autorisée pour un PDF Factur-X (20 Mo).</summary>
    public const long MaxPdfSizeBytes = 20L * 1024 * 1024;

    /// <summary>
    /// Lit intégralement le contenu d'un fichier uploadé dans un tableau d'octets.
    /// La limite de taille est volontaire et bornée pour un PDF Factur-X.
    /// </summary>
    [SuppressMessage("Security", "S5693:Content length limit", Justification = "Limite volontaire de 20 Mo pour un PDF Factur-X.")]
    public static async Task<byte[]> ReadAllBytesAsync(IBrowserFile file, CancellationToken cancellationToken = default)
    {
        using var stream = new MemoryStream();
        await file.OpenReadStream(MaxPdfSizeBytes, cancellationToken).CopyToAsync(stream, cancellationToken);
        return stream.ToArray();
    }
}
