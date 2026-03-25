using Rtl.Core.IntegrationTests;
using Xunit;

namespace Rtl.Core.IntegrationTests.Abstractions;

[CollectionDefinition(nameof(IntegrationTestCollection))]
public sealed class IntegrationTestCollection : ICollectionFixture<IntegrationTestFixture<Program>>;
