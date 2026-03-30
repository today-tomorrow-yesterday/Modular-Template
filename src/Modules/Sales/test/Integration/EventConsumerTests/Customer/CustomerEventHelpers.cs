using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.Customer.Application.Customers.SyncCustomerFromCrm;
using Modules.Customer.Domain.Customers.Enums;
using Modules.Sales.EventConsumerTests.Abstractions;
using Npgsql;
using Quartz;

namespace Modules.Sales.EventConsumerTests.Customer;

/// <summary>
/// Helpers for triggering Customer module events in Sales integration tests.
///
/// These mirror the integration event handlers in:
///   Sales/Presentation/IntegrationEvents/Customer/
///
/// Each helper sends a command to the Customer module, which raises domain events.
/// Call <see cref="PublishEventsFromOutboxAsync"/> after to process them through the pipeline.
/// </summary>
public static class CustomerEventHelpers
{
    private static string GetCustomerConnectionString(EventConsumerTestFixture fixture)
    {
        var config = fixture.Services.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>();
        return config["Modules:Customer:ConnectionStrings:Database"]
               ?? config.GetConnectionString("Database")
               ?? throw new InvalidOperationException("No Customer database connection string found in configuration.");
    }

    /// <summary>
    /// Creates a customer via CRM sync.
    /// Triggers: CustomerCreatedDomainEvent → CustomerCreatedIntegrationEvent
    /// Sales handler: CustomerCreatedIntegrationEventHandler → UpsertCustomerCacheCommand
    /// </summary>
    public static async Task CreateCustomerViaCrmSyncAsync(
        EventConsumerTestFixture fixture,
        int crmCustomerId = 99001,
        string firstName = "Jane",
        string lastName = "Doe")
    {
        using var scope = fixture.Services.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var command = new SyncCustomerFromCrmCommand(
            CrmCustomerId: crmCustomerId,
            HomeCenterNumber: EventConsumerTestFixture.TestHomeCenterNumber,
            LifecycleStage: LifecycleStage.Customer,
            FirstName: firstName,
            MiddleName: null,
            LastName: lastName,
            NameExtension: null,
            DateOfBirth: new DateOnly(1990, 1, 15),
            SalesAssignments: [],
            ContactPoints:
            [
                new SyncContactPointDto(ContactPointType.Email, $"{firstName.ToLower()}@test.com", true),
                new SyncContactPointDto(ContactPointType.Phone, "555-0100", false)
            ],
            Identifiers:
            [
                new SyncIdentifierDto(IdentifierType.CrmCustomerId, crmCustomerId.ToString())
            ],
            MailingAddress: null,
            SalesforceUrl: null,
            CreatedOn: DateTimeOffset.UtcNow,
            LastModifiedOn: DateTimeOffset.UtcNow);

        var result = await sender.Send(command);
        if (result.IsFailure)
        {
            throw new InvalidOperationException($"CreateCustomerViaCrmSync failed: {result.Error}");
        }
    }

    /// <summary>
    /// Updates an existing customer via CRM sync (same command, conditional domain events).
    /// The CrmCustomerId must match a previously created customer.
    ///
    /// Conditionally triggers (based on what changed):
    ///   - CustomerNameChangedDomainEvent → CustomerNameChangedIntegrationEvent
    ///   - CustomerHomeCenterChangedDomainEvent → CustomerHomeCenterChangedIntegrationEvent
    ///   - CustomerContactPointsChangedDomainEvent → CustomerContactPointsChangedIntegrationEvent
    ///   - CustomerMailingAddressChangedDomainEvent → CustomerMailingAddressChangedIntegrationEvent
    ///   - CustomerSalesAssignmentsChangedDomainEvent → CustomerSalesAssignmentsChangedIntegrationEvent
    /// </summary>
    public static async Task UpdateCustomerViaCrmSyncAsync(
        EventConsumerTestFixture fixture,
        int crmCustomerId,
        int? homeCenterNumber = null,
        string? firstName = null,
        string? lastName = null,
        SyncContactPointDto[]? contactPoints = null,
        SyncMailingAddressDto? mailingAddress = null,
        SyncSalesAssignmentDto[]? salesAssignments = null)
    {
        using var scope = fixture.Services.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var hc = homeCenterNumber ?? EventConsumerTestFixture.TestHomeCenterNumber;
        var fn = firstName ?? "Jane";

        var command = new SyncCustomerFromCrmCommand(
            CrmCustomerId: crmCustomerId,
            HomeCenterNumber: hc,
            LifecycleStage: LifecycleStage.Customer,
            FirstName: fn,
            MiddleName: null,
            LastName: lastName ?? "Doe",
            NameExtension: null,
            DateOfBirth: new DateOnly(1990, 1, 15),
            SalesAssignments: salesAssignments ?? [],
            ContactPoints: contactPoints ??
            [
                new SyncContactPointDto(ContactPointType.Email, $"{fn.ToLower()}@test.com", true),
                new SyncContactPointDto(ContactPointType.Phone, "555-0100", false)
            ],
            Identifiers:
            [
                new SyncIdentifierDto(IdentifierType.CrmCustomerId, crmCustomerId.ToString())
            ],
            MailingAddress: mailingAddress,
            SalesforceUrl: null,
            CreatedOn: DateTimeOffset.UtcNow,
            LastModifiedOn: DateTimeOffset.UtcNow);

        var result = await sender.Send(command);
        if (result.IsFailure)
            throw new InvalidOperationException($"UpdateCustomerViaCrmSync failed: {result.Error}");
    }

    /// <summary>
    /// Flushes the Customer module outbox by triggering its Quartz job.
    /// After this returns, all pending domain events have been processed
    /// and integration events dispatched via the in-memory bus.
    /// </summary>
    public static async Task PublishEventsFromOutboxAsync(EventConsumerTestFixture fixture)
    {
        var schedulerFactory = fixture.Services.GetRequiredService<ISchedulerFactory>();
        var scheduler = await schedulerFactory.GetScheduler();
        var jobKey = new JobKey("Modules.Customer.Infrastructure.Outbox.ProcessOutboxJob");
        await scheduler.TriggerJob(jobKey);

        // Poll until the outbox is empty (max 5 seconds)
        await using var conn = new NpgsqlConnection(GetCustomerConnectionString(fixture));
        await conn.OpenAsync();
        for (var i = 0; i < 50; i++)
        {
            await Task.Delay(100);
            await using var cmd = new NpgsqlCommand(
                "SELECT count(*) FROM messaging.outbox_messages WHERE processed_on_utc IS NULL", conn);
            var pending = (long)(await cmd.ExecuteScalarAsync())!;
            if (pending == 0) return;
        }
    }
}
