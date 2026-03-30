using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Modules.Customer.Application.Customers.SyncCustomerFromCrm;
using Modules.Customer.Domain.Customers.Enums;
using Modules.Customer.EventProducerTests.Abstractions;
using Modules.Customer.IntegrationEvents;

namespace Modules.Customer.EventProducerTests.CrmSync;

// Producer: SyncCustomerFromCrmCommand (update existing customer)
// Expected events: conditional based on what changed
// Each test creates a customer, flushes, clears spy, updates a field, flushes, asserts specific event
public class CustomerUpdatedProducerTests(EventProducerTestFixture fixture) : EventProducerTestBase(fixture)
{
    private const int CrmCustomerId = 78001;

    private async Task CreateBaseCustomerAsync()
    {
        using var scope = Fixture.Services.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var command = new SyncCustomerFromCrmCommand(
            CrmCustomerId: CrmCustomerId,
            HomeCenterNumber: 100,
            LifecycleStage: LifecycleStage.Customer,
            FirstName: "Bob",
            MiddleName: null,
            LastName: "Original",
            NameExtension: null,
            DateOfBirth: null,
            SalesAssignments: [],
            ContactPoints: [new SyncContactPointDto(ContactPointType.Email, "bob@test.com", true)],
            Identifiers: [new SyncIdentifierDto(IdentifierType.CrmCustomerId, CrmCustomerId.ToString())],
            MailingAddress: null,
            SalesforceUrl: null,
            CreatedOn: DateTimeOffset.UtcNow,
            LastModifiedOn: DateTimeOffset.UtcNow);

        var result = await sender.Send(command);
        Assert.True(result.IsSuccess);

        await Fixture.FlushOutboxAsync();
        Spy.Clear(); // Clear the Created event so we only see update events
    }

    private async Task UpdateCustomerAsync(
        string? firstName = null,
        string? lastName = null,
        int? homeCenterNumber = null,
        SyncContactPointDto[]? contactPoints = null,
        SyncMailingAddressDto? mailingAddress = null,
        SyncSalesAssignmentDto[]? salesAssignments = null,
        bool useDefaultMailingAddress = false)
    {
        using var scope = Fixture.Services.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var command = new SyncCustomerFromCrmCommand(
            CrmCustomerId: CrmCustomerId,
            HomeCenterNumber: homeCenterNumber ?? 100,
            LifecycleStage: LifecycleStage.Customer,
            FirstName: firstName ?? "Bob",
            MiddleName: null,
            LastName: lastName ?? "Original",
            NameExtension: null,
            DateOfBirth: null,
            SalesAssignments: salesAssignments ?? [],
            ContactPoints: contactPoints ?? [new SyncContactPointDto(ContactPointType.Email, "bob@test.com", true)],
            Identifiers: [new SyncIdentifierDto(IdentifierType.CrmCustomerId, CrmCustomerId.ToString())],
            MailingAddress: useDefaultMailingAddress ? null : mailingAddress,
            SalesforceUrl: null,
            CreatedOn: DateTimeOffset.UtcNow,
            LastModifiedOn: DateTimeOffset.UtcNow);

        var result = await sender.Send(command);
        Assert.True(result.IsSuccess);

        await Fixture.FlushOutboxAsync();
    }

    [Fact]
    public async Task UpdateName_ProducesCustomerNameChangedEvent()
    {
        await CreateBaseCustomerAsync();

        await UpdateCustomerAsync(firstName: "Robert", lastName: "Updated");

        Assert.True(Spy.HasEvent<CustomerNameChangedIntegrationEvent>());
        var evt = Spy.GetSingle<CustomerNameChangedIntegrationEvent>();
        Assert.Equal("Robert", evt.FirstName);
        Assert.Equal("Updated", evt.LastName);
        Assert.NotEqual(Guid.Empty, evt.PublicCustomerId);
    }

    [Fact]
    public async Task UpdateHomeCenter_ProducesCustomerHomeCenterChangedEvent()
    {
        await CreateBaseCustomerAsync();

        await UpdateCustomerAsync(homeCenterNumber: 200);

        Assert.True(Spy.HasEvent<CustomerHomeCenterChangedIntegrationEvent>());
        var evt = Spy.GetSingle<CustomerHomeCenterChangedIntegrationEvent>();
        Assert.Equal(200, evt.NewHomeCenterNumber);
        Assert.NotEqual(Guid.Empty, evt.PublicCustomerId);
    }

    [Fact]
    public async Task UpdateContactPoints_ProducesCustomerContactPointsChangedEvent()
    {
        await CreateBaseCustomerAsync();

        await UpdateCustomerAsync(contactPoints:
        [
            new SyncContactPointDto(ContactPointType.Email, "bob.new@test.com", true),
            new SyncContactPointDto(ContactPointType.Phone, "555-9999", false)
        ]);

        Assert.True(Spy.HasEvent<CustomerContactPointsChangedIntegrationEvent>());
        var evt = Spy.GetSingle<CustomerContactPointsChangedIntegrationEvent>();
        Assert.Contains(evt.ContactPoints, cp => cp.Type == "Email" && cp.Value == "bob.new@test.com");
        Assert.Contains(evt.ContactPoints, cp => cp.Type == "Phone" && cp.Value == "555-9999");
    }

    [Fact]
    public async Task UpdateMailingAddress_ProducesCustomerMailingAddressChangedEvent()
    {
        await CreateBaseCustomerAsync();

        await UpdateCustomerAsync(mailingAddress: new SyncMailingAddressDto(
            "200 Oak Ave", null, "Austin", "Travis", "TX", "US", "73301"));

        Assert.True(Spy.HasEvent<CustomerMailingAddressChangedIntegrationEvent>());
        var evt = Spy.GetSingle<CustomerMailingAddressChangedIntegrationEvent>();
        Assert.NotNull(evt.MailingAddress);
        Assert.Equal("Austin", evt.MailingAddress.City);
        Assert.Equal("TX", evt.MailingAddress.State);
    }

    [Fact]
    public async Task SyncWithNoChanges_ProducesNoEvents()
    {
        await CreateBaseCustomerAsync();

        // Re-sync with identical data
        await UpdateCustomerAsync();

        Assert.Empty(Spy.PublishedEvents);
    }
}
