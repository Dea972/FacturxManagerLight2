using FacturXManager.Common.Domain;
using FacturXManager.Modules.Directory.Application.Users.LookupChorusProUser;

namespace FacturXManager.Modules.Directory.Application.Abstractions;

/// <summary>
/// Port de consultation d'un utilisateur Chorus Pro via l'API PISTE /utilisateurs.
/// </summary>
public interface IChorusProUserService
{
    Task<Result<ChorusProUserLookupResponse>> ConsultUserAsync(
        string login,
        CancellationToken cancellationToken = default);
}
