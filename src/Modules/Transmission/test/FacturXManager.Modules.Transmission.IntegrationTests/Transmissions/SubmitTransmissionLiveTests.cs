using FacturXManager.Common.Domain;
using FacturXManager.Modules.Transmission.Application.Transmissions.SubmitTransmission;
using FacturXManager.Modules.Transmission.Infrastructure.ChorusPro;
using FacturXManager.Modules.Transmission.IntegrationTests.Abstractions;
using FacturXManager.Modules.Transmission.IntegrationTests.Abstractions.Builders;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FacturXManager.Modules.Transmission.IntegrationTests.Transmissions;

/// <summary>
/// Test de soumission contre la sandbox PISTE réelle — aucun mock HTTP.
/// Ignoré automatiquement si les credentials sandbox ne sont pas configurés
/// (user secrets ou variables d'environnement <c>Transmission:Piste:ClientId</c>).
/// </summary>
public class SubmitTransmissionLiveTests : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly IntegrationTestWebAppFactory _factory;

    public SubmitTransmissionLiveTests(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task SubmitTransmission_ShouldReachRealChorusPro_WhenSandboxCredentialsAreConfigured()
    {
        // Arrange : on crée une variante de l'application qui pointe vers la sandbox réelle
        // plutôt que vers WireMock. Une seule instance reconfigurée suffit.
        WebApplicationFactory<Program> liveFactory = _factory.WithWebHostBuilder(builder => builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Transmission:Piste:ChorusProBaseUrl"] = "https://sandbox-api.piste.gouv.fr",
            ["Transmission:Piste:TokenEndpoint"] = "https://sandbox-oauth.piste.gouv.fr/api/oauth/token",
        })));

        using IServiceScope scope = liveFactory.Services.CreateScope();
        PisteAuthOptions options = scope.ServiceProvider.GetRequiredService<IOptions<PisteAuthOptions>>().Value;

        if (string.IsNullOrWhiteSpace(options.ClientId) || string.IsNullOrWhiteSpace(options.ClientSecret))
        {
            // Pas de credentials sandbox configurés — skip proprement sans faire échouer le test.
            return;
        }

        ISender sender = scope.ServiceProvider.GetRequiredService<ISender>();
        SubmitTransmissionCommand command = new SubmitTransmissionCommandBuilder()
            .WithDocumentReference($"LIVE-TEST-{DateTime.UtcNow:yyyyMMddHHmmss}")
            .Build();

        // Act — appel HTTP réel vers la sandbox Chorus Pro / PISTE sans aucun mock réseau
        Result<SubmitTransmissionSummary> result = await sender.Send(command);

        // Assert : on accepte un dépôt réussi ou un rejet applicatif (ex. données fictives).
        // Un timeout ou 5xx serait un vrai problème d'infrastructure à investiguer.
        result.Error.Type.Should().NotBe(ErrorType.ExternalService,
            because: "une erreur technique côté PISTE indique un problème de connectivité");
        result.Error.Type.Should().NotBe(ErrorType.Timeout,
            because: "un timeout vers la sandbox PISTE est un problème d'infrastructure");
    }
}
