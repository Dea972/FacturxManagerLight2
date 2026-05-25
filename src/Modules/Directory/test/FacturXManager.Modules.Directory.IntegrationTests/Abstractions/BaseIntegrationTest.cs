using Bogus;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace FacturXManager.Modules.Directory.IntegrationTests.Abstractions;

[Collection(nameof(IntegrationTestCollection))]
public abstract class BaseIntegrationTest : IDisposable
{
    protected static readonly Faker Faker = new();

    protected readonly IServiceScope Scope;

    protected readonly ISender Sender;

    protected readonly IServiceProvider ServiceProvider;

    protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
    {
        _ = factory.CreateClient();
        Scope = factory.Services.CreateScope();
        ServiceProvider = Scope.ServiceProvider;
        Sender = ServiceProvider.GetRequiredService<ISender>();
    }

    public void Dispose()
    {
        Scope.Dispose();
        GC.SuppressFinalize(this);
    }
}
