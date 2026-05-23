using Bogus;
using FacturXManager.Modules.Compliance.Application.Abstractions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NSubstitute.ClearExtensions;

namespace FacturXManager.Modules.Compliance.IntegrationTests.Abstractions;

[Collection(nameof(IntegrationTestCollection))]
public abstract class BaseIntegrationTest : IDisposable
{
    protected static readonly Faker Faker = new();

    protected readonly IServiceScope Scope;

    protected readonly ISender Sender;

    protected readonly IServiceProvider ServiceProvider;

    protected readonly IFacturXPdfA3BRenderer PdfRenderer;

    protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
    {
        _ = factory.CreateClient();
        Scope = factory.Services.CreateScope();
        ServiceProvider = Scope.ServiceProvider;
        Sender = ServiceProvider.GetRequiredService<ISender>();
        PdfRenderer = factory.PdfRenderer;
        PdfRenderer.ClearReceivedCalls();
        PdfRenderer.ClearSubstitute();
    }

    public void Dispose()
    {
        Scope.Dispose();
        GC.SuppressFinalize(this);
    }
}
