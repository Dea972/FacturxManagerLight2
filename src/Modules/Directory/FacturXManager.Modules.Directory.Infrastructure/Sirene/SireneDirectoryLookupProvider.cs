using System.Net;
using FacturXManager.Common.Application.Caching;
using FacturXManager.Common.Domain;
using FacturXManager.Modules.Directory.Application.Abstractions;
using FacturXManager.Modules.Directory.Domain.Companies;
using FacturXManager.Modules.Directory.Infrastructure.Sirene.Dto;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Refit;

namespace FacturXManager.Modules.Directory.Infrastructure.Sirene;

/// <summary>
/// Adapter SIRENE du port <see cref="IDirectoryLookupProvider"/>.
/// Lit l'API INSEE via Refit, projette les DTO en value objects et met les résultats en cache
/// pour respecter le quota INSEE (30 req/min sans abonnement).
/// </summary>
internal sealed class SireneDirectoryLookupProvider(
    ISireneApi sireneApi,
    ICacheService cacheService,
    IOptions<SireneOptions> options,
    ILogger<SireneDirectoryLookupProvider> logger)
    : IDirectoryLookupProvider
{
    private const string CacheKeyPrefixSiren = "directory:sirene:siren:";
    private const string CacheKeyPrefixSiret = "directory:sirene:siret:";

    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(Math.Max(options.Value.CacheDurationMinutes, 1));

    public async Task<Result<CompanyDirectoryEntry>> LookupBySirenAsync(
        Siren siren,
        CancellationToken cancellationToken = default)
    {
        string cacheKey = CacheKeyPrefixSiren + siren.Value;

        CachedDirectoryEntry? cached = await cacheService.GetAsync<CachedDirectoryEntry>(cacheKey, cancellationToken);
        if (cached is not null)
        {
            return cached.ToDomain();
        }

        try
        {
            ApiResponse<SireneUniteLegaleResponse> response = await sireneApi.GetUniteLegaleAsync(siren.Value, cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return Result.Failure<CompanyDirectoryEntry>(CompanyErrors.NotFound(siren.Value));
            }

            if (!response.IsSuccessStatusCode || response.Content is null)
            {
                logger.LogWarning(
                    "Consultation SIRENE en échec pour le SIREN {Siren} (status {Status})",
                    siren.Value,
                    (int)response.StatusCode);

                return Result.Failure<CompanyDirectoryEntry>(CompanyErrors.LookupUnavailable);
            }

            Result<CompanyDirectoryEntry> mapped =
                SireneCompanyMapper.ToDirectoryEntry(response.Content.UniteLegale, siren.Value);

            if (mapped.IsSuccess)
            {
                await cacheService.SetAsync(cacheKey, CachedDirectoryEntry.From(mapped.Value), _cacheDuration, cancellationToken);
            }

            return mapped;
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return Result.Failure<CompanyDirectoryEntry>(CompanyErrors.LookupTimeout);
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "Erreur de transport SIRENE pour le SIREN {Siren}", siren.Value);
            return Result.Failure<CompanyDirectoryEntry>(CompanyErrors.LookupUnavailable);
        }
    }

    public async Task<Result<CompanyDirectoryEntry>> LookupBySiretAsync(
        Siret siret,
        CancellationToken cancellationToken = default)
    {
        string cacheKey = CacheKeyPrefixSiret + siret.Value;

        CachedDirectoryEntry? cached = await cacheService.GetAsync<CachedDirectoryEntry>(cacheKey, cancellationToken);
        if (cached is not null)
        {
            return cached.ToDomain();
        }

        try
        {
            ApiResponse<SireneEtablissementResponse> response = await sireneApi.GetEtablissementAsync(siret.Value, cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return Result.Failure<CompanyDirectoryEntry>(CompanyErrors.NotFound(siret.Value));
            }

            if (!response.IsSuccessStatusCode || response.Content is null)
            {
                logger.LogWarning(
                    "Consultation SIRENE en échec pour le SIRET {Siret} (status {Status})",
                    siret.Value,
                    (int)response.StatusCode);

                return Result.Failure<CompanyDirectoryEntry>(CompanyErrors.LookupUnavailable);
            }

            Result<CompanyDirectoryEntry> mapped =
                SireneCompanyMapper.ToDirectoryEntry(response.Content.Etablissement, siret.Value);

            if (mapped.IsSuccess)
            {
                await cacheService.SetAsync(cacheKey, CachedDirectoryEntry.From(mapped.Value), _cacheDuration, cancellationToken);
            }

            return mapped;
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return Result.Failure<CompanyDirectoryEntry>(CompanyErrors.LookupTimeout);
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "Erreur de transport SIRENE pour le SIRET {Siret}", siret.Value);
            return Result.Failure<CompanyDirectoryEntry>(CompanyErrors.LookupUnavailable);
        }
    }

    /// <summary>
    /// Représentation sérialisable d'un <see cref="CompanyDirectoryEntry"/> pour le cache distribué.
    /// Les value objects ayant des constructeurs privés, on passe par ce DTO et on reconstruit
    /// via les factories du Domain à la lecture.
    /// </summary>
    private sealed record CachedDirectoryEntry(
        string Siren,
        string? Siret,
        string? LegalName,
        CompanyLegalStatus Status,
        bool IsPubliclyDisclosed,
        DirectorySource Source,
        string? AttachedPlatformIdentifier)
    {
        internal static CachedDirectoryEntry From(CompanyDirectoryEntry entry) =>
            new(
                entry.Siren.Value,
                entry.Siret?.Value,
                entry.LegalName,
                entry.Status,
                entry.IsPubliclyDisclosed,
                entry.Source,
                entry.AttachedPlatformIdentifier);

        internal CompanyDirectoryEntry ToDomain()
        {
            Siren siren = Domain.Companies.Siren.Create(Siren).Value;
            Siret? siret = Siret is null ? null : Domain.Companies.Siret.Create(Siret).Value;

            return CompanyDirectoryEntry.Create(
                siren,
                siret,
                LegalName,
                Status,
                IsPubliclyDisclosed,
                Source,
                AttachedPlatformIdentifier);
        }
    }
}
