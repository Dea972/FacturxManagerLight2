using System.Reflection;

namespace FacturXManager.Modules.Directory.ArchitectureTests.Abstractions;

public abstract class BaseTest
{
    protected static readonly Assembly ApplicationAssembly =
        typeof(FacturXManager.Modules.Directory.Application.AssemblyReference).Assembly;

    protected static readonly Assembly InfrastructureAssembly =
        typeof(FacturXManager.Modules.Directory.Infrastructure.DirectoryModule).Assembly;

    protected static readonly Assembly PresentationAssembly =
        typeof(FacturXManager.Modules.Directory.Presentation.AssemblyReference).Assembly;
}
