using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using FacturXManager.Common.Presentation.Endpoints;
using FacturXManager.Modules.Directory.Application.Abstractions;
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
        services.AddTransient<SireneBearerAuthHandler>();

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
            .AddHttpMessageHandler<SireneBearerAuthHandler>();

        services.AddScoped<IDirectoryLookupProvider, SireneDirectoryLookupProvider>();
    }

    private static Task ConfigureAsync(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SireneOptions>(configuration.GetSection(SireneOptions.SectionName));

        return Task.CompletedTask;
    }
}
