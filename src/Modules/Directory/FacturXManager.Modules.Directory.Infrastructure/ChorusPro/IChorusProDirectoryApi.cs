using FacturXManager.Modules.Directory.Infrastructure.ChorusPro.Dto;
using Refit;

namespace FacturXManager.Modules.Directory.Infrastructure.ChorusPro;

/// <summary>
/// Client Refit de l'API Structures / Utilisateurs Chorus Pro (PISTE).
/// </summary>
internal interface IChorusProDirectoryApi
{
    /// <summary>
    /// Recherche une structure Chorus Pro par SIRET.
    /// </summary>
    [Post("/structures/v1/rechercher")]
    Task<ApiResponse<ChorusProSearchStructureResponse>> SearchStructureAsync(
        [Body] ChorusProSearchStructureRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retourne le détail d'une structure Chorus Pro à partir de son identifiant CPP.
    /// </summary>
    [Post("/structures/v1/consulter")]
    Task<ApiResponse<ChorusProConsultStructureResponse>> ConsultStructureAsync(
        [Body] ChorusProConsultStructureRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Consulte un utilisateur Chorus Pro à partir de son login.
    /// ⚠️ Nécessite des droits de gestionnaire sur la structure cible.
    /// </summary>
    [Post("/utilisateurs/v1/consulter")]
    Task<ApiResponse<ChorusProConsultUserResponse>> ConsultUserAsync(
        [Body] ChorusProConsultUserRequest request,
        CancellationToken cancellationToken = default);
}
