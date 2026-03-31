---
name: scaffold-integration-event
description: Use when adding a new integration event contract between modules - creates the event record, domain event handler that publishes it, and consumer handler in the target module
---

# Scaffold Integration Event

Creates a complete integration event pipeline: domain event → handler → integration event → consumer handler in target module.

## Rules (Non-Negotiable)

1. **NEVER include `int` entity IDs** — only `Guid Public{EntityName}Id`
2. **Naming:** `[EventDetailType("rtl.{sourceModule}.{camelCaseEventName}")]`
3. **Base:** extends `IntegrationEvent(Id, OccurredOnUtc)`
4. **Remove/delete events** carry ONLY the Guid identity — lean payload
5. **Add/change events** carry the full state snapshot consumers need for caching
6. **No "Ref" prefix** in event properties — Ref is internal domain naming

## Integration Event Record

**File:** `src/Modules/{SourceModule}/IntegrationEvents/{EventName}IntegrationEvent.cs`

### State event (Added, Changed, Revised):
```csharp
using Rtl.Core.Application.EventBus;

namespace Modules.{SourceModule}.IntegrationEvents;

[EventDetailType("rtl.{sourceModule}.{camelCaseEventName}")]
public sealed record {EventName}IntegrationEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    Guid Public{EntityName}Id,       // ALWAYS Guid, NEVER int
    // ... state fields the consumer needs (no Ref prefix)
) : IntegrationEvent(Id, OccurredOnUtc);
```

### Remove event (lean):
```csharp
[EventDetailType("rtl.{sourceModule}.{camelCaseEventName}")]
public sealed record {EventName}IntegrationEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    Guid Public{EntityName}Id) : IntegrationEvent(Id, OccurredOnUtc);
```

## Domain Event Handler (Producer)

**File:** `src/Modules/{SourceModule}/Application/{Feature}/EventHandlers/{DomainEvent}Handler.cs`

### For Add/Revise events (loads entity from DB):
```csharp
internal sealed class {DomainEvent}Handler(
    I{Entity}Repository repository,
    IEventBus eventBus,
    IDateTimeProvider dateTimeProvider) : DomainEventHandler<{DomainEvent}>
{
    public override async Task Handle(
        {DomainEvent} domainEvent,
        CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetByIdAsync(domainEvent.EntityId, cancellationToken);
        if (entity is null) return;

        await eventBus.PublishAsync(
            new {EventName}IntegrationEvent(
                Guid.CreateVersion7(),
                dateTimeProvider.UtcNow,
                entity.PublicId,           // ALWAYS PublicId, NEVER Id
                // ... map entity properties
            ),
            cancellationToken);
    }
}
```

### For Remove events (reads PublicId from domain event):
```csharp
internal sealed class {DomainEvent}Handler(
    IEventBus eventBus,
    IDateTimeProvider dateTimeProvider) : DomainEventHandler<{DomainEvent}>
{
    public override async Task Handle(
        {DomainEvent} domainEvent,
        CancellationToken cancellationToken = default)
    {
        await eventBus.PublishAsync(
            new {EventName}IntegrationEvent(
                Guid.CreateVersion7(),
                dateTimeProvider.UtcNow,
                domainEvent.PublicId),     // From domain event, not entity
            cancellationToken);
    }
}
```

## Consumer Handler (Target Module)

**File:** `src/Modules/{TargetModule}/Presentation/IntegrationEvents/{SourceModule}/{EventName}IntegrationEventHandler.cs`

```csharp
internal sealed class {EventName}IntegrationEventHandler(
    ISender sender,
    ILogger<{EventName}IntegrationEventHandler> logger)
    : IntegrationEventHandler<{EventName}IntegrationEvent>
{
    public override async Task HandleAsync(
        {EventName}IntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        logger.Log{Level}(
            "Processing {EventName}: Public{Entity}Id={Id}",
            integrationEvent.Public{EntityName}Id);

        await sender.Send(
            new {Command}(/* map event properties */),
            cancellationToken);
    }
}
```

**Log levels:**
- `LogInformation` for add/create/change events
- `LogWarning` for remove/delete events

## Naming Convention Reference

| Source Module | Entity | Event | Detail Type |
|---|---|---|---|
| SampleOrders | Order | OrderPlaced | `rtl.sampleOrders.orderPlaced` |
| SampleOrders | Order | OrderStatusChanged | `rtl.sampleOrders.orderStatusChanged` |
| SampleSales | Product | ProductCreated | `rtl.sampleSales.productCreated` |
| SampleSales | Product | ProductUpdated | `rtl.sampleSales.productUpdated` |

## Checklist

- [ ] Event record has `Guid Public{EntityName}Id` — no int IDs
- [ ] `[EventDetailType("rtl.{module}.{camelCase}")]` attribute present
- [ ] Remove events carry ONLY the Guid
- [ ] Producer handler maps `entity.PublicId` (not `entity.Id`) for add/revise events
- [ ] Producer handler reads `domainEvent.PublicId` for remove events
- [ ] Consumer handler logs at appropriate level (Info for add, Warning for remove)
- [ ] No "Ref" prefix on event properties
