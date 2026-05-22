namespace FacturXManager.Common.Application.AI;

/// <summary>
/// Abstraction for AI embedding generation services.
/// Implement this interface in the infrastructure layer to generate vector embeddings
/// for text inputs, useful for semantic search, RAG, and similarity matching.
/// </summary>
public interface IEmbeddingService
{
    /// <summary>
    /// Generates an embedding vector for the given text input.
    /// </summary>
    /// <param name="text">The text to generate an embedding for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The embedding vector as a float array.</returns>
    Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates embedding vectors for multiple text inputs in a batch.
    /// </summary>
    /// <param name="texts">The texts to generate embeddings for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of embedding vectors.</returns>
    Task<IReadOnlyList<float[]>> GenerateEmbeddingsAsync(IEnumerable<string> texts, CancellationToken cancellationToken = default);
}
