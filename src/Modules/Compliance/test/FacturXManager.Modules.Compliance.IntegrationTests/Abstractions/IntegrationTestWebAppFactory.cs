using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ChaussonStore.Modules.Catalog.IntegrationTests.Abstractions;

/// <summary>
/// Web application factory that uses real external services (Algolia, etc.)
/// instead of mocks. Use this for end-to-end integration tests.
/// </summary>
public sealed class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
 

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

    }

    public async Task InitializeAsync()
    {
    }

    public new async Task DisposeAsync()
    {
    }
}


