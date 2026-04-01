using ModularTemplate.IntegrationTests;
using Xunit;

namespace ModularTemplate.IntegrationTests.Abstractions;

[CollectionDefinition(nameof(IntegrationTestCollection))]
public sealed class IntegrationTestCollection : ICollectionFixture<IntegrationTestFixture<Program>>;
