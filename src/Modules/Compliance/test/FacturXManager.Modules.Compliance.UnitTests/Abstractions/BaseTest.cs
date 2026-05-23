using Bogus;

namespace FacturXManager.Modules.Compliance.UnitTests.Abstractions;

public abstract class BaseTest
{
    protected static readonly Faker Faker = new();
}
