using ChaussonStore.Modules.Catalog.IntegrationTests.Abstractions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ChaussonStore.Modules.Customers.IntegrationTests.Abstractions;

[Collection(nameof(IntegrationTestCollection))]
public abstract class BaseIntegrationTest : IDisposable
{
    protected readonly IServiceScope Scope;
    protected readonly ISender Sender;
    protected readonly IServiceProvider ServiceProvider;

    protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
    {
        _ = factory.CreateClient();
        Scope = factory.Services.CreateScope();
        Sender = Scope.ServiceProvider.GetRequiredService<ISender>();
        ServiceProvider = Scope.ServiceProvider;
    }

    public void Dispose()
    {
        Scope.Dispose();
        GC.SuppressFinalize(this);
    }
}
