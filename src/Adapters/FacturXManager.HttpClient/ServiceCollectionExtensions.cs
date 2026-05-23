using System;
using FacturXManager.Client.Compliance;
using FacturXManager.Client.EReporting;
using FacturXManager.Client.Transmission;
using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace FacturXManager.Client;

/// <summary>
/// Extension methods pour enregistrer le client HTTP FacturX Manager dans le conteneur DI.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Enregistre tous les clients Refit (Compliance, Transmission, EReporting).
    /// </summary>
    /// <param name="services">Le conteneur DI.</param>
    /// <param name="baseUrl">URL de base de l'API FacturX Manager (ex: https://localhost:7000).</param>
    /// <param name="configure">Configuration optionnelle des settings Refit.</param>
    public static IServiceCollection AddFacturXManagerClient(
        this IServiceCollection services,
        Uri baseUrl,
        Action<RefitSettings>? configure = null)
    {
        RefitSettings settings = BuildSettings(configure);

        services
            .AddRefitClient<IComplianceApi>(settings)
            .ConfigureHttpClient(c => c.BaseAddress = baseUrl);

        services
            .AddRefitClient<ITransmissionApi>(settings)
            .ConfigureHttpClient(c => c.BaseAddress = baseUrl);

        services
            .AddRefitClient<IEReportingApi>(settings)
            .ConfigureHttpClient(c => c.BaseAddress = baseUrl);

        return services;
    }

    /// <summary>
    /// Enregistre tous les clients Refit à partir d'une URL de base fournie en chaîne.
    /// </summary>
    public static IServiceCollection AddFacturXManagerClient(
        this IServiceCollection services,
        string baseUrl,
        Action<RefitSettings>? configure = null)
        => services.AddFacturXManagerClient(new Uri(baseUrl), configure);

    private static RefitSettings BuildSettings(Action<RefitSettings>? configure)
    {
        var settings = new RefitSettings();
        configure?.Invoke(settings);
        return settings;
    }
}
