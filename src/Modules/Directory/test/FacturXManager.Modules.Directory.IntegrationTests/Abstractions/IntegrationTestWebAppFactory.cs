using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace FacturXManager.Modules.Directory.IntegrationTests.Abstractions;

/// <summary>
/// Compose l'API in-memory pour les tests d'intégration du module Directory.
/// Les appels sont envoyés à la vraie API SIRENE (INSEE) — aucun mock réseau.
/// </summary>
public sealed class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddUserSecrets<Program>();
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Database"] = "Server=test;Database=facturxmanager_tests;Integrated Security=True;",
                ["Directory:Sirene:BaseUrl"] = "https://api.insee.fr/api-sirene/3.11",
                ["Directory:Sirene:CacheDurationMinutes"] = "0",
            });
        });
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public new Task DisposeAsync() => Task.CompletedTask;
}
