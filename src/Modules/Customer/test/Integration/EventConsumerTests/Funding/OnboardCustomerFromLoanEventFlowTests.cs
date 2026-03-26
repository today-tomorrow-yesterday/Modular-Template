using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Modules.Customer.Application.Customers.GetCustomerByPublicId;
using Modules.Customer.EventConsumerTests.Abstractions;
using Modules.Customer.Infrastructure.Persistence;
using Modules.Customer.Integration.Shared;
using Rtl.Core.Presentation.Results;

namespace Modules.Customer.EventConsumerTests.Funding;

// OnboardCustomerFromLoanRequestedIntegrationEvent (from Funding module)
//
// Flow: Funding publishes event → InMemoryEventBus dispatches synchronously →
//       OnboardCustomerFromLoanRequestedIntegrationEventHandler →
//       OnboardCustomerFromLoanCommand → customer created in DB
//
// Tests:
// - Event published → customer exists in DB and is retrievable via GET endpoint
public class OnboardCustomerFromLoanEventFlowTests(EventConsumerTestFixture fixture)
    : EventConsumerTestBase(fixture)
{
    [Fact]
    public async Task OnboardFromLoan_EventFlow_CustomerCreatedInDatabase()
    {
        // Arrange & Act — publish the Funding integration event
        // InMemoryEventBus dispatches synchronously, so the handler runs immediately
        await FundingEventHelpers.PublishOnboardCustomerFromLoanEventAsync(Fixture);

        // Find the created customer's PublicId in the database
        using var scope = Fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CustomerDbContext>();
        var customer = await db.Set<Domain.Customers.Entities.Customer>()
            .FirstOrDefaultAsync(c => c.HomeCenterNumber == TestHomeCenterNumber);

        Assert.NotNull(customer); // Customer should have been created by the event handler
        var publicId = customer.PublicId;

        // Verify the customer is retrievable via the GET endpoint
        var response = await Client.GetAsync($"/api/v1/customers/{publicId}");
        var body = await response.Content.ReadFromJsonAsync<ApiEnvelope<CustomerResponse>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);                    // Should return 200 OK
        Assert.NotNull(body?.Data);                                               // Should have customer data
        Assert.Equal(publicId, body.Data.PublicId);                               // Should match the created customer
        Assert.Equal(FakeVmfLosAdapter.DefaultFirstName, body.Data.FirstName);    // Should match borrower first name
        Assert.Equal(FakeVmfLosAdapter.DefaultLastName, body.Data.LastName);      // Should match borrower last name
        Assert.Equal(TestHomeCenterNumber, body.Data.HomeCenterNumber);           // Should match home center
    }
}
