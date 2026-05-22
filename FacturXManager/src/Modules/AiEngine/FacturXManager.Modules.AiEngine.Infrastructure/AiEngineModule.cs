using System.Reflection;
using FacturXManager.Common.Application.EventBus;
using FacturXManager.Common.Presentation.Endpoints;
using FacturXManager.Modules.AiEngine.Application.Abstractions.Data;
using FacturXManager.Modules.AiEngine.Domain.ConversationStatistics;
using FacturXManager.Modules.AiEngine.Infrastructure.ConversationStatistics;
using FacturXManager.Modules.AiEngine.Infrastructure.Database;
using FacturXManager.Modules.AiEngine.Infrastructure.Inbox;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FacturXManager.Modules.AiEngine.Infrastructure;

public static class AiEngineModule
{
    public static async Task<IServiceCollection> AddAiEngineModuleAsync(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMediatR(config => config.RegisterServicesFromAssembly(Assembly.GetAssembly(typeof(AiEngineModule))!));


        services.AddIntegrationEventHandlers();

        services.AddInfrastructure();

        services.AddEndpoints(Presentation.AssemblyReference.Assembly);

        await services.ConfigureAsync(configuration);

        return services;
    }

    private static void AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IConversationStatisticRepository, ConversationStatisticRepository>();
        services.AddScoped<ILlmCallRepository, LlmCallRepository>();

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AiEngineDbContext>());
    }

    private static void AddIntegrationEventHandlers(this IServiceCollection services)
    {
        Type[] integrationEventHandlers = Presentation.AssemblyReference.Assembly
            .GetTypes()
            .Where(t => t.IsAssignableTo(typeof(IIntegrationEventHandler)))
            .ToArray();

        foreach (Type integrationEventHandler in integrationEventHandlers)
        {
            services.TryAddScoped(integrationEventHandler);

            Type integrationEvent = integrationEventHandler
                .GetInterfaces()
                .Single(i => i.IsGenericType)
                .GetGenericArguments()
                .Single();

            Type closedIdempotentHandler =
                typeof(IdempotentIntegrationEventHandler<>).MakeGenericType(integrationEvent);

            services.Decorate(integrationEventHandler, closedIdempotentHandler);
        }
    }

    private static Task ConfigureAsync(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<InboxOptions>(configuration.GetSection("AiEngine:Inbox"));

        services.ConfigureOptions<ConfigureProcessInboxJob>();

        return Task.CompletedTask;
    }
}
