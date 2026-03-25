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
using Rtl.Core.Presentation.Results;
using Quartz;

namespace Modules.Sales.IntegrationTests.Events;

/// <summary>
/// End-to-end event flow: Customer module → domain event → outbox → event bus → Sales cache.
///
/// Proves the full pipeline works by using the cached data in a real business operation:
/// 1. SyncCustomerFromCrmCommand creates a customer in customer_dev
/// 2. Outbox processes CustomerCreatedDomainEvent → publishes CustomerCreatedIntegrationEvent
/// 3. Sales handler receives it and upserts cache.customers
/// 4. POST /api/v1/sales with that customer succeeds (proves the cache is populated and usable)
/// </summary>
[Collection("SalesIntegration")]
public sealed class CustomerCreatedEventFlowTests(SalesIntegrationTestFixture fixture) : IAsyncLifetime
{
    private readonly IServiceScope _scope = fixture.Services.CreateScope();
    private readonly HttpClient _client = fixture.CreateClient();

    public async Task InitializeAsync() => await fixture.ResetDatabaseAsync();

    public Task DisposeAsync()
    {
        _client.Dispose();
        _scope.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task CustomerCreated_EventFlow_SaleCanBeCreatedWithCachedCustomer()
    {
        // Step 1 — Create customer in Customer module via command
        var sender = _scope.ServiceProvider.GetRequiredService<ISender>();
        var command = new SyncCustomerFromCrmCommand(
            CrmPartyId: 99001,
            HomeCenterNumber: 100,
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
        var customerDb = _scope.ServiceProvider.GetRequiredService<CustomerDbContext>();
        var customer = await customerDb.Set<Modules.Customer.Domain.Customers.Entities.Customer>()
            .FirstAsync(c => c.HomeCenterNumber == 100);
        var customerPublicId = customer.PublicId;

        // Step 2 — Flush the Customer outbox (triggers full event pipeline)
        var schedulerFactory = _scope.ServiceProvider.GetRequiredService<ISchedulerFactory>();
        var scheduler = await schedulerFactory.GetScheduler();
        await scheduler.TriggerJob(new JobKey("Modules.Customer.Infrastructure.Outbox.ProcessOutboxJob"));
        await Task.Delay(500);

        // Step 3 — Prove the cache works: create a sale using the event-populated customer
        // If cache.customers wasn't populated by the event, this returns 404 Customer.NotFound
        var response = await _client.PostAsJsonAsync("/api/v1/sales",
            new CreateSaleRequest(customerPublicId, 100));

        // Assert — sale was created successfully (proves the event pipeline worked)
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiEnvelope<CreateSaleResponse>>();
        Assert.NotNull(body?.Data);
        Assert.NotEqual(Guid.Empty, body.Data.Id);
        Assert.True(body.Data.SaleNumber > 0);
    }
}
