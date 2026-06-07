using System.Net;
using FacturXManager.Common.Domain;
using FacturXManager.Modules.Directory.Application.Abstractions;
using FacturXManager.Modules.Directory.Application.Companies.LookupCompany;
using FacturXManager.Modules.Directory.Application.Structures.ConsultChorusProStructure;
using FacturXManager.Modules.Directory.Domain.Companies;
using FacturXManager.Modules.Directory.Infrastructure.ChorusPro.Dto;
using Microsoft.Extensions.Logging;
using Refit;

namespace FacturXManager.Modules.Directory.Infrastructure.ChorusPro;

/// <summary>
/// Implémentation de <see cref="IChorusProStructureService"/> via l'API PISTE /structures.
/// </summary>
internal sealed class ChorusProStructureService(
    IChorusProDirectoryApi api,
    ILogger<ChorusProStructureService> logger)
    : IChorusProStructureService
{
    public async Task<Result<CompanyLookupResponse>> SearchBySiretAsync(
        string siret,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Recherche structure Chorus Pro (PISTE /structures/v1/rechercher) pour le SIRET {Siret}",
            siret);

        try
        {
            ApiResponse<ChorusProSearchStructureResponse> response = await api.SearchStructureAsync(
                new ChorusProSearchStructureRequest { Siret = siret },
                cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound || response.Content?.ResultCount == 0)
            {
                return Result.Failure<CompanyLookupResponse>(CompanyErrors.NotFound(siret));
            }

            if (!response.IsSuccessStatusCode || response.Content is null)
            {
                logger.LogWarning(
                    "Recherche structure Chorus Pro en échec pour {Siret} (status {Status})",
                    siret,
                    (int)response.StatusCode);

                return Result.Failure<CompanyLookupResponse>(CompanyErrors.LookupUnavailable);
            }

            ChorusProStructureSummary? structure = response.Content.Structures.FirstOrDefault();
            if (structure is null)
            {
                return Result.Failure<CompanyLookupResponse>(CompanyErrors.NotFound(siret));
            }

            return MapToLookupResponse(structure, siret);
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return Result.Failure<CompanyLookupResponse>(CompanyErrors.LookupTimeout);
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "Erreur de transport Chorus Pro pour le SIRET {Siret}", siret);
            return Result.Failure<CompanyLookupResponse>(CompanyErrors.LookupUnavailable);
        }
    }

    public async Task<Result<ChorusProStructureDetailResponse>> ConsultByIdAsync(
        long idStructureCpp,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Consultation structure Chorus Pro (PISTE /structures/v1/consulter) pour l'id CPP {IdStructureCpp}",
            idStructureCpp);

        try
        {
            ApiResponse<ChorusProConsultStructureResponse> response = await api.ConsultStructureAsync(
                new ChorusProConsultStructureRequest { IdStructureCpp = idStructureCpp },
                cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return Result.Failure<ChorusProStructureDetailResponse>(
                    CompanyErrors.NotFound(idStructureCpp.ToString()));
            }

            if (!response.IsSuccessStatusCode || response.Content is null)
            {
                logger.LogWarning(
                    "Consultation structure Chorus Pro en échec pour id CPP {IdStructureCpp} (status {Status})",
                    idStructureCpp,
                    (int)response.StatusCode);

                return Result.Failure<ChorusProStructureDetailResponse>(CompanyErrors.LookupUnavailable);
            }

            ChorusProConsultStructureResponse detail = response.Content;

            return new ChorusProStructureDetailResponse(
                detail.IdStructureCpp,
                detail.Siret,
                detail.RaisonSociale,
                detail.CodeStatut,
                detail.LibStatut,
                detail.IdentifiantTechnique,
                detail.TypeIdentifiantStructure,
                detail.CodeServiceDlin,
                detail.LibServiceDlin,
                detail.NombreServices);
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return Result.Failure<ChorusProStructureDetailResponse>(CompanyErrors.LookupTimeout);
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "Erreur de transport Chorus Pro pour l'id CPP {IdStructureCpp}", idStructureCpp);
            return Result.Failure<ChorusProStructureDetailResponse>(CompanyErrors.LookupUnavailable);
        }
    }

    private static Result<CompanyLookupResponse> MapToLookupResponse(ChorusProStructureSummary structure, string siret)
    {
        Result<Siren> sirenResult = Siren.Create(siret[..9]);
        if (sirenResult.IsFailure)
        {
            return Result.Failure<CompanyLookupResponse>(sirenResult.Error);
        }

        bool isActive = structure.CodeStatut is "A" or "a";

        return new CompanyLookupResponse(
            Siren: sirenResult.Value.Value,
            Siret: siret,
            LegalName: structure.RaisonSociale,
            Status: isActive ? nameof(CompanyLegalStatus.Active) : nameof(CompanyLegalStatus.Ceased),
            IsActive: isActive,
            IsPubliclyDisclosed: true,
            Source: nameof(DirectorySource.Ppf),
            AttachedPlatformIdentifier: structure.IdentifiantTechnique ?? structure.IdStructureCpp.ToString());
    }
}
