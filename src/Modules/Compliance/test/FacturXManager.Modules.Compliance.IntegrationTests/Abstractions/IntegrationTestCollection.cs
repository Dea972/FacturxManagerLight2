using ChaussonStore.Modules.Catalog.IntegrationTests.Abstractions;

namespace ChaussonStore.Modules.Customers.IntegrationTests.Abstractions;

[CollectionDefinition(nameof(IntegrationTestCollection))]
public sealed class IntegrationTestCollection : ICollectionFixture<IntegrationTestWebAppFactory>;
