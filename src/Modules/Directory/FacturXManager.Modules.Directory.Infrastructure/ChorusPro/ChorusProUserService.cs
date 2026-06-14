using System.Net;
using FacturXManager.Common.Domain;
using FacturXManager.Modules.Directory.Application.Abstractions;
using FacturXManager.Modules.Directory.Application.Users.LookupChorusProUser;
using FacturXManager.Modules.Directory.Domain.Companies;
using FacturXManager.Modules.Directory.Infrastructure.ChorusPro.Dto;
using Microsoft.Extensions.Logging;
using Refit;

namespace FacturXManager.Modules.Directory.Infrastructure.ChorusPro;

/// <summary>
/// Implémentation de <see cref="IChorusProUserService"/> via l'API PISTE /utilisateurs.
/// </summary>
internal sealed class ChorusProUserService(
    IChorusProDirectoryApi api,
    ILogger<ChorusProUserService> logger)
    : IChorusProUserService
{
    public async Task<Result<ChorusProUserLookupResponse>> ConsultUserAsync(
        string login,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Consultation utilisateur Chorus Pro (PISTE /utilisateurs/v1/consulter) pour le login {Login}",
            login);

        try
        {
            ApiResponse<ChorusProConsultUserResponse> response = await api.ConsultUserAsync(
                new ChorusProConsultUserRequest { Login = login },
                cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return Result.Failure<ChorusProUserLookupResponse>(CompanyErrors.NotFound(login));
            }

            if (!response.IsSuccessStatusCode || response.Content is null)
            {
                logger.LogWarning(
                    "Consultation utilisateur Chorus Pro en échec pour {Login} (status {Status})",
                    login,
                    (int)response.StatusCode);

                return Result.Failure<ChorusProUserLookupResponse>(CompanyErrors.LookupUnavailable);
            }

            ChorusProConsultUserResponse user = response.Content;

            return new ChorusProUserLookupResponse(
                Login: user.Login ?? login,
                Nom: user.Nom,
                Prenom: user.Prenom,
                Email: user.Email,
                Actif: user.Actif,
                IdUtilisateurCourant: user.IdUtilisateurCourant,
                Structures: user.Structures
                    .Select(s => new ChorusProUserStructureResponse(s.IdStructureCpp, s.RaisonSociale, s.Siret))
                    .ToList());
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return Result.Failure<ChorusProUserLookupResponse>(CompanyErrors.LookupTimeout);
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "Erreur de transport Chorus Pro pour l'utilisateur {Login}", login);
            return Result.Failure<ChorusProUserLookupResponse>(CompanyErrors.LookupUnavailable);
        }
    }
}
