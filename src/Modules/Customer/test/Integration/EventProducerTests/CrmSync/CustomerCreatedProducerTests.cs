using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Modules.Customer.Application.Customers.SyncCustomerFromCrm;
using Modules.Customer.Domain.Customers.Enums;
using Modules.Customer.EventProducerTests.Abstractions;
using Modules.Customer.IntegrationEvents;

namespace Modules.Customer.EventProducerTests.CrmSync;

// Producer: SyncCustomerFromCrmCommand (new customer)
// Expected event: CustomerCreatedIntegrationEvent
// Verified: event payload matches command data (name, HC, lifecycle, contacts, identifiers, mailing address)
public class CustomerCreatedProducerTests(EventProducerTestFixture fixture) : EventProducerTestBase(fixture)
{
    [Fact]
    public async Task SyncNewCustomer_ProducesCustomerCreatedEvent_WithCorrectPayload()
    {
        // Arrange -- send command to create a customer
        using var scope = Fixture.Services.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var command = new SyncCustomerFromCrmCommand(
            CrmPartyId: 77001,
            HomeCenterNumber: 100,
            LifecycleStage: LifecycleStage.Lead,
            FirstName: "Alice",
            MiddleName: "M",
            LastName: "Producer",
            NameExtension: null,
            DateOfBirth: new DateOnly(1985, 6, 15),
            SalesAssignments: [],
            ContactPoints:
            [
                new SyncContactPointDto(ContactPointType.Email, "alice@test.com", true),
                new SyncContactPointDto(ContactPointType.Phone, "555-7700", false)
            ],
            Identifiers:
            [
                new SyncIdentifierDto(IdentifierType.CrmPartyId, "77001"),
                new SyncIdentifierDto(IdentifierType.SalesforceAccountId, "SF-ALICE-001")
            ],
            MailingAddress: new SyncMailingAddressDto("100 Main St", null, "Nashville", "Davidson", "TN", "US", "37201"),
            SalesforceUrl: "https://sf.example.com/alice",
            CreatedOn: DateTimeOffset.UtcNow,
            LastModifiedOn: DateTimeOffset.UtcNow);

        var result = await sender.Send(command);
        Assert.True(result.IsSuccess);

        // Act -- flush outbox to trigger domain event handler -> publishes integration event
        await Fixture.FlushOutboxAsync();

        // Assert -- SpyEventBus should have captured exactly one CustomerCreatedIntegrationEvent
        Assert.True(Spy.HasEvent<CustomerCreatedIntegrationEvent>());
        var evt = Spy.GetSingle<CustomerCreatedIntegrationEvent>();

        Assert.Equal("Alice", evt.FirstName);
        Assert.Equal("M", evt.MiddleName);
        Assert.Equal("Producer", evt.LastName);
        Assert.Equal(100, evt.HomeCenterNumber);
        Assert.Equal("Lead", evt.LifecycleStage);
        Assert.NotEqual(Guid.Empty, evt.CustomerId);

        // Verify contact points made it through
        Assert.Contains(evt.ContactPoints, cp => cp.Type == "Email" && cp.Value == "alice@test.com");
        Assert.Contains(evt.ContactPoints, cp => cp.Type == "Phone" && cp.Value == "555-7700");

        // Verify identifiers
        Assert.Contains(evt.Identifiers, id => id.Type == "SalesforceAccountId" && id.Value == "SF-ALICE-001");

        // Verify mailing address
        Assert.NotNull(evt.MailingAddress);
        Assert.Equal("Nashville", evt.MailingAddress.City);
        Assert.Equal("TN", evt.MailingAddress.State);
    }
}
