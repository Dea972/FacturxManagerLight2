using FacturXManager.Common.Application.AI;
using FacturXManager.Common.Application.EventBus;
using FacturXManager.Modules.AiEngine.Application.Abstractions.Data;
using FacturXManager.Modules.AiEngine.Domain.ConversationStatistics;

namespace FacturXManager.Modules.AiEngine.Presentation.ConversationStatistics;

internal sealed class LlmCalledIntegrationEventHandler(
    IUnitOfWork unitOfWork,
    IConversationStatisticRepository conversationStatisticRepository)
    : IntegrationEventHandler<LlmCalledIntegrationEvent>
{
    public override async Task Handle(LlmCalledIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        ConversationStatistic? conversation =
           await conversationStatisticRepository.GetByConversationIdAsync(integrationEvent.SessionId, cancellationToken);

        if (conversation is null)
        {
            conversation = ConversationStatistic.Create(
                integrationEvent.SessionId,
                integrationEvent.UserId,
                integrationEvent.SourceModule);

            conversationStatisticRepository.Insert(conversation);
        }

        var llmCall = LlmCall.Create(
            conversation.Id,
            integrationEvent.AgentName,
            integrationEvent.ModelName,
            integrationEvent.ModelProvider,
            integrationEvent.OperationType,
            integrationEvent.InputTokens,
            integrationEvent.InputCachedTokens,
            integrationEvent.OutputTokens,
            integrationEvent.OutputReasoningTokens,
            integrationEvent.TotalTokens,
            integrationEvent.ResponseTimeMs,
            integrationEvent.IsSuccess,
            integrationEvent.ErrorMessage,
            integrationEvent.OccurredOnUtc);

        conversation.AddLlmCall(llmCall);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
