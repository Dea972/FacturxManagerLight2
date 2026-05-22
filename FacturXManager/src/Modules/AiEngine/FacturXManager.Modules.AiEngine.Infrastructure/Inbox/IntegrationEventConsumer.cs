using System.Data.Common;
using FacturXManager.Common.Application.Data;
using FacturXManager.Common.Application.EventBus;
using FacturXManager.Common.Infrastructure.Inbox;
using FacturXManager.Common.Infrastructure.Serialization;
using FacturXManager.Modules.AiEngine.Infrastructure.Database;
using Dapper;
using MassTransit;
using Newtonsoft.Json;

namespace FacturXManager.Modules.AiEngine.Infrastructure.Inbox;

internal sealed class IntegrationEventConsumer<TIntegrationEvent>(IDbConnectionFactory dbConnectionFactory)
    : IConsumer<TIntegrationEvent>
    where TIntegrationEvent : IntegrationEvent
{
    public async Task Consume(ConsumeContext<TIntegrationEvent> context)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        TIntegrationEvent integrationEvent = context.Message;

        var inboxMessage = new InboxMessage
        {
            Id = integrationEvent.Id,
            Type = integrationEvent.GetType().Name,
            Content = JsonConvert.SerializeObject(integrationEvent, SerializerSettings.Instance),
            OccurredOnUtc = integrationEvent.OccurredOnUtc
        };

        string sql =
            $"""
            INSERT INTO {Schemas.AiEngine}.inbox_messages(id, type, content, occurred_on_utc)
            VALUES (@Id, @Type, @Content, @OccurredOnUtc)
            """;

        await connection.ExecuteAsync(sql, inboxMessage);
    }
}
