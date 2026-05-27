using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using FacturXManager.Common.Application.Transmission;
using FacturXManager.Common.Presentation.Endpoints;
using FacturXManager.Modules.Transmission.Application.Abstractions.Data;
using FacturXManager.Modules.Transmission.Domain.Transmissions;
using FacturXManager.Modules.Transmission.Infrastructure.ChorusPro;
using FacturXManager.Modules.Transmission.Infrastructure.Database;
using FacturXManager.Modules.Transmission.Infrastructure.Transmissions;
using FacturXManager.Modules.Transmission.Infrastructure.Transmitters;
using Refit;

namespace FacturXManager.Modules.Transmission.Infrastructure;

public static class TransmissionModule
{
    public static async Task<IServiceCollection> AddTransmissionModuleAsync(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMediatR(config =>
            config.RegisterServicesFromAssembly(Application.AssemblyReference.Assembly));

        services.AddValidatorsFromAssembly(
            Application.AssemblyReference.Assembly,
            includeInternalTypes: true);

        services.AddInfrastructure(configuration);

        services.AddEndpoints(Presentation.AssemblyReference.Assembly);

        await services.ConfigureAsync(configuration);

        return services;
    }

    private static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("Database") ?? string.Empty;

        services.AddDbContext<TransmissionDbContext>(options =>
            options
                .UseSqlServer(connectionString, sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", Schemas.Transmission))
                .UseSnakeCaseNamingConvention());

        services.AddScoped<ITransmissionRepository, TransmissionRepository>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<TransmissionDbContext>());

        services.AddTransient<PisteAuthenticationHandler>();
        services.AddTransient<CproAccountHandler>();
        services.AddHttpClient("piste-auth");

        services
            .AddRefitClient<IChorusProApi>()
            .ConfigureHttpClient((sp, client) =>
            {
                PisteAuthOptions options = sp.GetRequiredService<IOptions<PisteAuthOptions>>().Value;
                if (!string.IsNullOrWhiteSpace(options.ChorusProBaseUrl))
                {
                    client.BaseAddress = new Uri(options.ChorusProBaseUrl);
                }
            })
            .AddHttpMessageHandler<PisteAuthenticationHandler>()
            .AddHttpMessageHandler<CproAccountHandler>();

        services.AddScoped<IChannelTransmitter, ChorusProTransmitter>();
        services.AddScoped<IInvoiceTransmitter, CompositeInvoiceTransmitter>();
    }

    private static Task ConfigureAsync(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<PisteAuthOptions>(configuration.GetSection(PisteAuthOptions.SectionName));

        return Task.CompletedTask;
    }
}
