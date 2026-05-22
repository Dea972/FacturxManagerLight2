using System;
using System.Collections.Generic;
using System.Text;
using FacturXManager.Common.Application.FileHelpers;
using FacturXManager.Common.Domain;

namespace FacturXManager.Common.Application.AI;

/// <summary>
/// Interface générique pour les interactions avec les LLM (Large Language Models).
/// Chaque module consommant un LLM doit passer par cette abstraction.
/// </summary>
internal interface ILlmService
{
    /// <summary>
    /// Crée une conversation côté fournisseur et retourne son identifiant.
    /// </summary>
    Task<Result<string>> CreateConversationAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Envoie un message et retourne la réponse complète avec les métriques de tokens.
    /// </summary>
    Task<LlmResponse> SendMessageAsync(
        LlmRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Envoie un message et retourne un flux asynchrone de chunks (streaming).
    /// </summary>
    IAsyncEnumerable<LlmStreamChunk> SendStreamAsync(
        LlmRequest request,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Requête générique vers un LLM.
/// </summary>
public sealed record LlmRequest(
    string SessionId,
    string Message,
    string? UserId = null,
    string? SourceModule = null,
    string? OperationType = null,
    IEnumerable<FileAttachment>? Attachments = null);

/// <summary>
/// Réponse complète d'un LLM avec métriques de tokens.
/// </summary>
public sealed record LlmResponse(
    string Content,
    LlmTokenUsage TokenUsage,
    string AgentName,
    string ModelName,
    string? ModelProvider = null);

/// <summary>
/// Métriques d'utilisation des tokens.
/// </summary>
public sealed record LlmTokenUsage(
    int InputTokens,
    int InputCachedTokens,
    int OutputTokens,
    int OutputReasoningTokens,
    int TotalTokens);

/// <summary>
/// Pièce jointe dans une requête LLM.
/// </summary>
public sealed record LlmAttachment(
    string FileName,
    byte[] Content,
    string MimeType);

/// <summary>
/// Chunk unitaire retourné lors d'un streaming LLM.
/// </summary>
public sealed record LlmStreamChunk(
    string Content,
    bool IsComplete,
    LlmTokenUsage? TokenUsage = null,
    string? AgentName = null,
    string? ModelName = null,
    string? ModelProvider = null);
