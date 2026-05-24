using FacturXManager.Modules.Directory.Application.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;

namespace FacturXManager.Modules.Directory.IntegrationTests.Abstractions;

/// <summary>
/// Compose l'API in-memory pour les tests d'intégration du module Directory.
/// L'annuaire externe (SIRENE, demain PPF) est mocké via <see cref="IDirectoryLookupProvider"/>.
/// Le module est sans persistance ; aucun DbContext n'est requis.
/// </summary>
public sealed class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public IDirectoryLookupProvider LookupProvider { get; } = Substitute.For<IDirectoryLookupProvider>();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Database"] = "Server=test;Database=facturxmanager_tests;Integrated Security=True;",
        }));

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IDirectoryLookupProvider>();
            services.AddSingleton(LookupProvider);
        });
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public new Task DisposeAsync() => Task.CompletedTask;
}
