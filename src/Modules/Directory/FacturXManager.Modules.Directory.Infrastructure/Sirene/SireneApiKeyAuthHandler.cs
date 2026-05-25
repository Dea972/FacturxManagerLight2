using System.Net.Http.Headers;
using Microsoft.Extensions.Options;

namespace FacturXManager.Modules.Directory.Infrastructure.Sirene;

/// <summary>
/// Injecte le token Bearer SIRENE sur chaque appel sortant. Le token est lu via
/// <see cref="IOptions{TOptions}"/> et n'est jamais journalisé.
/// </summary>
internal sealed class SireneApiKeyAuthHandler(IOptions<SireneOptions> options) : DelegatingHandler
{
    private readonly SireneOptions _options = options.Value;

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            request.Headers.Add("X-INSEE-Api-Key-Integration", _options.ApiKey);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
