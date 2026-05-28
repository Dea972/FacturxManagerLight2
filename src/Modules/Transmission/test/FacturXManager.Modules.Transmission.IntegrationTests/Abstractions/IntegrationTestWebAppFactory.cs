using FacturXManager.Modules.Transmission.Infrastructure.Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace FacturXManager.Modules.Transmission.IntegrationTests.Abstractions;

/// <summary>
/// Compose l'API in-memory pour les tests d'intégration du module Transmission.
/// <list type="bullet">
///   <item>Base de données SQLite in-memory nommée — même provider que l'application.</item>
///   <item><see cref="WireMockServer"/> exposé : les tests pilotent les réponses HTTP Chorus Pro
///   en enregistrant des stubs via <c>Factory.WireMock</c>.</item>
///   <item>Aucun mock NSubstitute sur le transmitter : le vrai <c>ChorusProTransmitter</c>
///   est utilisé et cible le serveur WireMock.</item>
/// </list>
/// </summary>
public sealed class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly string _dbName = $"transmission-tests-{Guid.NewGuid():N}";

    /// <summary>Serveur WireMock démarré sur un port libre. Disponible pour tous les tests.</summary>
    public WireMockServer WireMock { get; } = WireMockServer.Start();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddUserSecrets<Program>();
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                // Base SQLite in-memory nommée — même provider que l'application, connection string seulement.
                ["ConnectionStrings:Database"] = $"DataSource={_dbName};Mode=Memory;Cache=Shared",

                // Redirige tous les appels Chorus Pro vers WireMock.
                ["Transmission:Piste:ChorusProBaseUrl"] = WireMock.Urls[0],

                // OAuth2 : WireMock intercepte et renvoie un token fictif.
                ["Transmission:Piste:TokenEndpoint"] = $"{WireMock.Urls[0]}/oauth/token",
            });
        });
    }

    public async Task InitializeAsync()
    {
        // Stub OAuth2 minimal pour que PisteAuthenticationHandler obtienne un token.
        WireMock
            .Given(Request.Create().WithPath("/oauth/token").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("""{"access_token":"test-token","expires_in":3600,"token_type":"Bearer"}"""));

        // Crée le schéma sur la base SQLite in-memory.
        await using AsyncServiceScope scope = Services.CreateAsyncScope();
        TransmissionDbContext db = scope.ServiceProvider.GetRequiredService<TransmissionDbContext>();
        await db.Database.EnsureCreatedAsync();
    }

    public new async Task DisposeAsync()
    {
        WireMock.Stop();
        await base.DisposeAsync();
    }
}

