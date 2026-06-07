using FacturXManager.Common.Application.Messaging;
using FacturXManager.Modules.Compliance.Application.Abstractions.Invoices;

namespace FacturXManager.Modules.Compliance.Application.Invoicing.EmbedFacturX;

/// <summary>
/// Commande pour embarquer le format Factur-X (XML CII conforme EN16931 et métadonnées XMP)
/// dans un fichier PDF visuel existant fourni par l'appelant.
/// </summary>
/// <param name="VisualPdfBytes">Les octets bruts du fichier PDF source à enrichir.</param>
/// <param name="Invoice">Le modèle logique (scalaire) du contenu de la facture.</param>
public sealed record EmbedFacturXCommand(
    byte[] VisualPdfBytes,
    InvoiceModel Invoice) : ICommand<byte[]>;
