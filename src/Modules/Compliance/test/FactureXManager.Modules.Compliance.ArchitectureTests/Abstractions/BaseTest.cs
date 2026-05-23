using System.Reflection;
using FacturXManager.Modules.Compliance.Infrastructure;

namespace FacturXManager.Modules.Compliance.ArchitectureTests.Abstractions;

public abstract class BaseTest
{
    protected static readonly Assembly ApplicationAssembly = typeof(Compliance.Application.AssemblyReference).Assembly;

    protected static readonly Assembly InfrastructureAssembly = typeof(ComplianceModule).Assembly;

    protected static readonly Assembly PresentationAssembly = typeof(Compliance.Presentation.AssemblyReference).Assembly;
}
