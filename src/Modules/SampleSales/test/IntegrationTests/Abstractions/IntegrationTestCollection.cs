using Rtl.Core.IntegrationTests;
using Rtl.Core.IntegrationTests.Abstractions;

namespace Modules.SampleSales.IntegrationTests.Abstractions;

/// <summary>
/// Collection fixture for SampleSales module integration tests.
/// Uses the shared IntegrationTestFixture from Common.IntegrationTests.
/// </summary>
[CollectionDefinition(nameof(IntegrationTestCollection))]
public sealed class IntegrationTestCollection : ICollectionFixture<IntegrationTestFixture<Program>>;
