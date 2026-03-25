using Rtl.Core.IntegrationTests.Abstractions;

namespace Modules.Sales.IntegrationTests.Abstractions;

[CollectionDefinition("SalesIntegration")]
public sealed class SalesIntegrationTestCollection : ICollectionFixture<SalesTestFactory>;
