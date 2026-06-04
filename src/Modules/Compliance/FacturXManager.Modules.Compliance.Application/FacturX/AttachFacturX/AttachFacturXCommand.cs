using FacturXManager.Common.Application.Messaging;
using FacturXManager.Modules.Compliance.Application.Abstractions.Invoices;

namespace FacturXManager.Modules.Compliance.Application.FacturX.AttachFacturX;

/// <summary>
/// Commande pour attacher le format Factur-X (XML CII conforme EN16931 et métadonnées)
/// à un fichier PDF visuel existant fourni.
/// </summary>
/// <param name="VisualPdfBytes">Les octets bruts du fichier PDF source.</param>
/// <param name="Invoice">Le modèle logique (scalaire) du contenu de la facture.</param>
public sealed record AttachFacturXCommand(
    byte[] VisualPdfBytes,
    InvoiceModel Invoice) : ICommand<AttachFacturXSummary>;
