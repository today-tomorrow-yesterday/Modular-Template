using System.Net;
using System.Net.Http.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Modules.Customer.Application.Customers.SyncCustomerFromCrm;
using Modules.Customer.Domain.Customers.Enums;
using Modules.Customer.Infrastructure.Persistence;
using Modules.Sales.IntegrationTests.Abstractions;
using Modules.Sales.Presentation.Endpoints.V1.Sales;
using Rtl.Core.IntegrationTests;
using Rtl.Core.Presentation.Results;

namespace Modules.Sales.IntegrationTests.Events;

/// <summary>
/// End-to-end event flow: Customer module → domain event → outbox → event bus → Sales cache.
///
/// Proves the pipeline works by creating a customer via the Customer module, flushing the
/// outbox, then using the cached customer in a real Sales business operation (CreateSale).
/// If the event pipeline failed at any step, CreateSale returns 404 Customer.NotFound.
/// </summary>
public class CustomerCreatedEventFlowTests(SalesIntegrationTestFixture factory)
    : SalesIntegrationTestBase(factory)
{
    [Fact]
    public async Task CustomerCreated_EventFlow_SaleCanBeCreatedWithCachedCustomer()
    {
        // Step 1 — Create a customer in the Customer module
        var sender = Fixture.Services.CreateScope().ServiceProvider.GetRequiredService<ISender>();
        var command = new SyncCustomerFromCrmCommand(
            CrmPartyId: 99001,
            HomeCenterNumber: TestHomeCenterNumber,
            LifecycleStage: Modules.Customer.Domain.Customers.Enums.LifecycleStage.Customer,
            FirstName: "Jane",
            MiddleName: null,
            LastName: "Doe",
            NameExtension: null,
            DateOfBirth: new DateOnly(1990, 1, 15),
            SalesAssignments: [],
            ContactPoints:
            [
                new SyncContactPointDto(ContactPointType.Email, "jane@test.com", true),
                new SyncContactPointDto(ContactPointType.Phone, "555-0100", false)
            ],
            Identifiers:
            [
                new SyncIdentifierDto(IdentifierType.CrmPartyId, "42")
            ],
            MailingAddress: null,
            SalesforceUrl: null,
            CreatedOn: DateTimeOffset.UtcNow,
            LastModifiedOn: DateTimeOffset.UtcNow);

        var result = await sender.Send(command);
        Assert.True(result.IsSuccess);

        // Get the customer's PublicId from the Customer DB (producer side)
        var customerDb = Fixture.Services.CreateScope().ServiceProvider
            .GetRequiredService<CustomerDbContext>();
        var customer = await customerDb.Set<Modules.Customer.Domain.Customers.Entities.Customer>()
            .FirstAsync(c => c.HomeCenterNumber == TestHomeCenterNumber);
        var customerPublicId = customer.PublicId;

        // Step 2 — Flush the Customer outbox (triggers full event pipeline)
        await Fixture.FlushCustomerOutboxAsync();

        // Step 3 — Prove the cache works: create a sale using the event-populated customer.
        // If cache.customers wasn't populated, this returns 404 Customer.NotFound.
        var response = await Client.PostAsJsonAsync("/api/v1/sales",
            new CreateSaleRequest(customerPublicId, TestHomeCenterNumber));

        await HttpAssert.IsCreatedAsync(response);
        var body = await response.Content.ReadFromJsonAsync<ApiEnvelope<CreateSaleResponse>>();
        Assert.NotNull(body?.Data);
        Assert.NotEqual(Guid.Empty, body.Data.Id);
        Assert.True(body.Data.SaleNumber > 0);
    }
}
