using System.ComponentModel.DataAnnotations;

namespace FacturXManager.BackOffice.Web.Features.Administration.ViewModels;

/// <summary>
/// ViewModel du formulaire de configuration des credentials PISTE / Chorus Pro.
/// Les secrets ne sont jamais affichés en clair après enregistrement (masked).
/// </summary>
public sealed class CredentialsFormViewModel
{
    // ── PISTE OAuth2 ──
    [Required(ErrorMessage = "Le Client ID PISTE est obligatoire.")]
    public string PisteClientId { get; set; } = string.Empty;

    /// <summary>Vide après premier enregistrement : affiché masqué.</summary>
    public string? PisteClientSecret { get; set; }

    // ── Compte Technique Chorus Pro ──
    [Required(ErrorMessage = "L'identifiant du compte technique est obligatoire.")]
    public string ChorusProAccountId { get; set; } = string.Empty;

    public string? ChorusProPassword { get; set; }

    [Required(ErrorMessage = "Le login Chorus Pro est obligatoire.")]
    public string ChorusProLogin { get; set; } = string.Empty;

    // ── Environnement ──
    [Required]
    public string Environment { get; set; } = "Sandbox";
}
