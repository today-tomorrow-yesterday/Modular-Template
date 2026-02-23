using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Newtonsoft.Json;
using Rtl.Core.Domain.Entities;
using Rtl.Core.Domain.Events;
using Rtl.Core.Infrastructure.Serialization;

namespace Rtl.Core.Infrastructure.Outbox.Persistence;

/// <summary>
/// EF Core interceptor that captures domain events and inserts them into the outbox.
/// </summary>
public sealed class InsertOutboxMessagesInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
        {
            InsertOutboxMessages(eventData.Context);
        }

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            InsertOutboxMessages(eventData.Context);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void InsertOutboxMessages(DbContext context)
    {
        var outboxMessages = context
            .ChangeTracker
            .Entries<Entity>()
            .SelectMany(entry =>
            {
                var entity = entry.Entity;
                var domainEvents = entity.DomainEvents;
                entity.ClearDomainEvents();

                // Enrich each event with the source entity's ID (assigned by Hi-Lo sequence at Add time)
                return domainEvents.Select(e => (DomainEvent)e with { EntityId = entity.Id });
            })
            .Select(domainEvent => new OutboxMessage
            {
                Id = domainEvent.Id,
                Type = domainEvent.GetType().Name,
                Content = JsonConvert.SerializeObject(domainEvent, SerializerSettings.Instance),
                OccurredOnUtc = domainEvent.OccurredOnUtc
            })
            .ToList();

        context.Set<OutboxMessage>().AddRange(outboxMessages);
    }
}
