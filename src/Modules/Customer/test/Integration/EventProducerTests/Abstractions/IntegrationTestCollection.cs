using Modules.Customer.EventProducerTests.Abstractions;

namespace Modules.Customer.EventProducerTests;

[CollectionDefinition("CustomerEventProducer")]
public sealed class IntegrationTestCollection : ICollectionFixture<EventProducerTestFixture>;
