using System.Diagnostics;
using System.Runtime.CompilerServices;
using FacturXManager.Common.Application.EventBus;
using FacturXManager.Common.Domain;

namespace FacturXManager.Common.Application.AI;

/// <summary>
/// Classe abstraite implémentant le pattern Template Method pour les appels LLM.
/// Gère automatiquement la publication de <see cref="LlmCalledIntegrationEvent"/> 
/// après chaque appel, le chronométrage et la gestion d'erreurs.
/// Les implémentations concrètes n'ont qu'à surcharger les méthodes *Internal.
/// </summary>
public abstract class LlmServiceBase(IEventBus eventBus) : ILlmService
{
    public Task<Result<string>> CreateConversationAsync(CancellationToken cancellationToken = default)
        => CreateConversationInternalAsync(cancellationToken);

    public async Task<LlmResponse> SendMessageAsync(
        LlmRequest request,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            LlmResponse response = await SendMessageInternalAsync(request, cancellationToken);

            stopwatch.Stop();

            await PublishLlmCalledEventAsync(
                request,
                response.TokenUsage,
                response.AgentName,
                response.ModelName,
                response.ModelProvider,
                stopwatch.ElapsedMilliseconds,
                isSuccess: true,
                errorMessage: null,
                cancellationToken);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            await PublishLlmCalledEventAsync(
                request,
                tokenUsage: null,
                agentName: null,
                modelName: "unknown",
                modelProvider: null,
                stopwatch.ElapsedMilliseconds,
                isSuccess: false,
                errorMessage: ex.Message,
                cancellationToken);

            throw;
        }
    }

    public async IAsyncEnumerable<LlmStreamChunk> SendStreamAsync(
        LlmRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        LlmStreamChunk? lastChunk = null;

        bool success = true;
        string? errorMessage = null;

        IAsyncEnumerable<LlmStreamChunk> stream;

        try
        {
            stream = SendStreamInternalAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            await PublishLlmCalledEventAsync(
                request,
                tokenUsage: null,
                agentName: null,
                modelName: "unknown",
                modelProvider: null,
                stopwatch.ElapsedMilliseconds,
                isSuccess: false,
                errorMessage: ex.Message,
                cancellationToken);

            throw;
        }

        await using IAsyncEnumerator<LlmStreamChunk> enumerator = stream.GetAsyncEnumerator(cancellationToken);

        while (true)
        {
            try
            {
                if (!await enumerator.MoveNextAsync())
                {
                    break;
                }
            }
            catch (Exception ex)
            {
                success = false;
                errorMessage = ex.Message;
                break;
            }

            lastChunk = enumerator.Current;
            yield return lastChunk;
        }

        stopwatch.Stop();

        await PublishLlmCalledEventAsync(
            request,
            lastChunk?.TokenUsage,
            lastChunk?.AgentName,
            lastChunk?.ModelName ?? "unknown",
            lastChunk?.ModelProvider,
            stopwatch.ElapsedMilliseconds,
            isSuccess: success,
            errorMessage: errorMessage,
            cancellationToken);

        if (!success)
        {
            throw new InvalidOperationException($"LLM stream failed: {errorMessage}");
        }
    }

    /// <summary>
    /// Crée une conversation côté fournisseur. À implémenter par le provider concret.
    /// </summary>
    protected abstract Task<Result<string>> CreateConversationInternalAsync(
        CancellationToken cancellationToken);

    /// <summary>
    /// Envoie un message au LLM. À implémenter par le provider concret.
    /// </summary>
    protected abstract Task<LlmResponse> SendMessageInternalAsync(
        LlmRequest request,
        CancellationToken cancellationToken);

    /// <summary>
    /// Envoie un message en streaming au LLM. À implémenter par le provider concret.
    /// </summary>
    protected abstract IAsyncEnumerable<LlmStreamChunk> SendStreamInternalAsync(
        LlmRequest request,
        CancellationToken cancellationToken);

    private async Task PublishLlmCalledEventAsync(
        LlmRequest request,
        LlmTokenUsage? tokenUsage,
        string? agentName,
        string modelName,
        string? modelProvider,
        long responseTimeMs,
        bool isSuccess,
        string? errorMessage,
        CancellationToken cancellationToken)
    {
        var integrationEvent = new LlmCalledIntegrationEvent(
            id: Guid.NewGuid(),
            sessionId: request.SessionId,
            userId: request.UserId,
            agentName: agentName,
            modelName: modelName,
            modelProvider: modelProvider,
            inputTokens: tokenUsage?.InputTokens ?? 0,
            inputCachedTokens: tokenUsage?.InputCachedTokens ?? 0,
            outputTokens: tokenUsage?.OutputTokens ?? 0,
            outputReasoningTokens: tokenUsage?.OutputReasoningTokens ?? 0,
            totalTokens: tokenUsage?.TotalTokens ?? 0,
            responseTimeMs: responseTimeMs,
            sourceModule: request.SourceModule,
            operationType: request.OperationType,
            isSuccess: isSuccess,
            errorMessage: errorMessage,
            occurredOnUtc: DateTime.UtcNow);

        await eventBus.PublishAsync(integrationEvent, cancellationToken);
    }
}
