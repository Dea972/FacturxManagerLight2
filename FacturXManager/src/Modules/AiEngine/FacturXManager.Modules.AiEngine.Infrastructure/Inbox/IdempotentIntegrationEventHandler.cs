using System.Data.Common;
using FacturXManager.Common.Application.Data;
using FacturXManager.Common.Application.EventBus;
using FacturXManager.Common.Infrastructure.Inbox;
using FacturXManager.Modules.AiEngine.Infrastructure.Database;
using Dapper;

namespace FacturXManager.Modules.AiEngine.Infrastructure.Inbox;

internal sealed class IdempotentIntegrationEventHandler<TIntegrationEvent>(
    IIntegrationEventHandler<TIntegrationEvent> decorated,
    IDbConnectionFactory dbConnectionFactory)
    : IntegrationEventHandler<TIntegrationEvent>
    where TIntegrationEvent : IIntegrationEvent
{
    public override async Task Handle(
        TIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        var inboxMessageConsumer = new InboxMessageConsumer(integrationEvent.Id, decorated.GetType().Name);

        if (await InboxConsumerExistsAsync(connection, inboxMessageConsumer))
        {
            return;
        }

        await decorated.Handle(integrationEvent, cancellationToken);

        await InsertInboxConsumerAsync(connection, inboxMessageConsumer);
    }

    private static async Task<bool> InboxConsumerExistsAsync(
        DbConnection dbConnection,
        InboxMessageConsumer inboxMessageConsumer)
    {
        string sql;
        if (dbConnection.GetType().Name.Contains("SqlConnection", StringComparison.OrdinalIgnoreCase))
        {
            // SQL Server syntax
            sql =
                $"""
                SELECT CASE WHEN EXISTS(
                    SELECT 1
                    FROM {Schemas.AiEngine}.inbox_message_consumers
                    WHERE inbox_message_id = @InboxMessageId AND
                          name = @Name
                ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END
                """;
        }
        else
        {
            // Assume PostgreSQL syntax
            sql =
                $"""
                SELECT EXISTS(
                    SELECT 1
                    FROM {Schemas.AiEngine}.inbox_message_consumers
                    WHERE inbox_message_id = @InboxMessageId AND
                          name = @Name
                )
                """;
        }

        return await dbConnection.ExecuteScalarAsync<bool>(sql, inboxMessageConsumer);
    }

    private static async Task InsertInboxConsumerAsync(
        DbConnection dbConnection,
        InboxMessageConsumer inboxMessageConsumer)
    {
        const string sql =
            $"""
            INSERT INTO {Schemas.AiEngine}.inbox_message_consumers(inbox_message_id, name)
            VALUES (@InboxMessageId, @Name)
            """;

        await dbConnection.ExecuteAsync(sql, inboxMessageConsumer);
    }
}
