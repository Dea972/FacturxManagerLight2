using Bogus;

namespace FacturXManager.Modules.Directory.UnitTests.Abstractions;

public abstract class BaseTest
{
    protected static readonly Faker Faker = new();
}
