namespace FacturXManager.BackOffice.Web.Features.Transmission;

/// <summary>
/// Résultat affichable d'un dépôt Chorus Pro.
/// </summary>
public sealed record TransmissionResultViewModel(
    Guid TransmissionId,
    string Status,
    string? ChannelDepositId,
    string? Message);
