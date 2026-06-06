using Refit;

namespace FacturXManager.BackOffice.Web.Common;

/// <summary>
/// Convertit les exceptions Refit/HTTP en messages d'erreur lisibles pour l'utilisateur.
/// </summary>
public static class ApiErrorTranslator
{
    private static readonly System.Text.Json.JsonSerializerOptions JsonOptions =
        new(System.Text.Json.JsonSerializerDefaults.Web);

    public static string Describe(Exception exception) => exception switch
    {
        ValidationApiException validation =>
            validation.Content?.Detail
            ?? validation.Content?.Title
            ?? "La requête est invalide.",
        ApiException api =>
            ExtractProblemDetail(api) ?? $"Erreur API ({(int)api.StatusCode}) : {api.ReasonPhrase}",
        HttpRequestException =>
            "Impossible de joindre l'API FacturX Manager. Vérifiez que le backend est démarré.",
        TaskCanceledException =>
            "La requête a expiré. Réessayez ultérieurement.",
        _ => exception.Message,
    };

    private static string? ExtractProblemDetail(ApiException api)
    {
        if (string.IsNullOrWhiteSpace(api.Content))
        {
            return null;
        }

        try
        {
            ProblemDetails? problem = System.Text.Json.JsonSerializer.Deserialize<ProblemDetails>(
                api.Content,
                JsonOptions);

            return problem?.Detail ?? problem?.Title;
        }
        catch (System.Text.Json.JsonException)
        {
            return null;
        }
    }

    private sealed record ProblemDetails(string? Title, string? Detail);
}
