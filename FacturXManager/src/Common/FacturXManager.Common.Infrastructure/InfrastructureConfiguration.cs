using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using FacturXManager.Common.Application.Caching;
using FacturXManager.Common.Application.Data;
using FacturXManager.Common.Application.Clock;
using FacturXManager.Common.Infrastructure.Caching;
using FacturXManager.Common.Infrastructure.Clock;
using FacturXManager.Common.Infrastructure.Data;
using FacturXManager.Common.Infrastructure.Outbox;
using Quartz;

namespace FacturXManager.Common.Infrastructure;
public static class InfrastructureConfiguration
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString,
        Action<IRegistrationConfigurator>[] moduleConfigureConsumers)
    {
        services.TryAddSingleton<IDateTimeProvider, DateTimeProvider>();

        services.AddDistributedMemoryCache();


        services.TryAddScoped<IDbConnectionFactory>(provider =>
            new DbConnectionFactory(connectionString));
        services.TryAddScoped<IDbConnectionFactory, DbConnectionFactory>();

        services.TryAddSingleton<ICacheService, CacheService>();

        services.AddQuartz(configurator =>
        {
            var scheduler = Guid.NewGuid();
            configurator.SchedulerId = $"default-id-{scheduler}";
            configurator.SchedulerName = $"default-name-{scheduler}";
        });


        services.AddDistributedMemoryCache(); // This requires the Microsoft.Extensions.Caching.Memory namespace


        services.TryAddSingleton<InsertOutboxMessagesInterceptor>();

        services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

        services.AddMassTransit(configure =>
        {
            foreach (Action<IRegistrationConfigurator> configureConsumers in moduleConfigureConsumers)
            {
                configureConsumers(configure);
            }

            configure.SetKebabCaseEndpointNameFormatter();

            configure.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
        });

        return services;
    }
}
