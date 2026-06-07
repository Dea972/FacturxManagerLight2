using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using FacturXManager.Common.Infrastructure.Piste;
using FacturXManager.Common.Presentation.Endpoints;
using FacturXManager.Modules.Directory.Application.Abstractions;
using FacturXManager.Modules.Directory.Infrastructure.ChorusPro;
using FacturXManager.Modules.Directory.Infrastructure.Sirene;
using Refit;

namespace FacturXManager.Modules.Directory.Infrastructure;

public static class DirectoryModule
{
    public static async Task<IServiceCollection> AddDirectoryModuleAsync(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMediatR(config =>
            config.RegisterServicesFromAssembly(Application.AssemblyReference.Assembly));

        services.AddValidatorsFromAssembly(
            Application.AssemblyReference.Assembly,
            includeInternalTypes: true);

        services.AddInfrastructure();

        services.AddEndpoints(Presentation.AssemblyReference.Assembly);

        await services.ConfigureAsync(configuration);

        return services;
    }

    private static void AddInfrastructure(this IServiceCollection services)
    {
        // SIRENE (INSEE)
        services.AddTransient<SireneApiKeyAuthHandler>();

        services
            .AddRefitClient<ISireneApi>()
            .ConfigureHttpClient((sp, client) =>
            {
                SireneOptions options = sp.GetRequiredService<IOptions<SireneOptions>>().Value;
                if (!string.IsNullOrWhiteSpace(options.BaseUrl))
                {
                    client.BaseAddress = new Uri(options.BaseUrl);
                }
            })
            .AddHttpMessageHandler<SireneApiKeyAuthHandler>();

        services.AddScoped<IDirectoryLookupProvider, SireneDirectoryLookupProvider>();

        // Chorus Pro (PISTE)
        services.AddTransient<PisteAuthenticationHandler>();
        services.AddTransient<CproAccountHandler>();

        // Le client HTTP "piste-auth" peut être partagé avec le module Transmission
        // si déjà enregistré ; AddHttpClient est idempotent sur le nom.
        services.AddHttpClient("piste-auth");

        services
            .AddRefitClient<IChorusProDirectoryApi>()
            .ConfigureHttpClient((sp, client) =>
            {
                ChorusProOptions options = sp.GetRequiredService<IOptions<ChorusProOptions>>().Value;
                if (!string.IsNullOrWhiteSpace(options.ChorusProBaseUrl))
                {
                    client.BaseAddress = new Uri(options.ChorusProBaseUrl);
                }
            })
            .AddHttpMessageHandler<PisteAuthenticationHandler>()
            .AddHttpMessageHandler<CproAccountHandler>();

        services.AddScoped<IChorusProStructureService, ChorusProStructureService>();
        services.AddScoped<IChorusProUserService, ChorusProUserService>();
    }

    private static Task ConfigureAsync(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SireneOptions>(configuration.GetSection(SireneOptions.SectionName));
        services.Configure<ChorusProOptions>(configuration.GetSection(ChorusProOptions.SectionName));
        services.Configure<PisteAuthOptions>(configuration.GetSection(PisteAuthOptions.SectionName));

        return Task.CompletedTask;
    }
}
