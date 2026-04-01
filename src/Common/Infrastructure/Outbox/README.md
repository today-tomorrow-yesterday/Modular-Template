# Outbox Pattern

## What Is It?

The **Transactional Outbox** ensures reliable domain event publishing. When a domain entity changes, events are persisted to an `outbox_messages` table in the same transaction as the entity change. A background job then processes and dispatches these events.

This guarantees **at-least-once delivery** - events are never lost even if the application crashes.

## How It Works

```
1. Entity changes + events saved in same transaction
2. Background job polls outbox_messages table
3. Events dispatched to handlers
4. Message marked as processed (or scheduled for retry)
```

## What's in This Folder?

| File | Purpose |
|------|---------|
| `OutboxOptions.cs` | Configuration (interval, batch size, max retries) |
| `ProcessOutboxJobBase.cs` | Abstract base job - modules inherit from this |
| `ConfigureProcessOutboxJob.cs` | Quartz job configuration helper |
| `IdempotentDomainEventHandlerBase.cs` | Base decorator for idempotent handling |
| `InsertOutboxMessagesInterceptor.cs` | EF interceptor that captures domain events |
| `DomainEventHandlersFactory.cs` | Discovers handlers via reflection |
| `Persistence/OutboxMessage.cs` | The outbox message entity |
| `Persistence/OutboxMessageConsumer.cs` | Tracks which handlers processed each message |
| `Configurations/*.cs` | EF Core entity configurations |

## Retry & Dead Letter

Failed messages are retried with exponential backoff:
- **Delays:** 5s → 30s → 2min → 10min → 1hr
- **Max retries:** 5 (configurable)
- **Dead letter:** After max retries, message is marked processed with error

## Feature Flag

Outbox processing can be disabled via the `InfrastructureFeatures.Outbox` feature flag. When disabled, the background job will skip processing and log a debug message. This is useful for:
- Temporarily pausing event processing during maintenance
- Debugging event handling issues
- Testing scenarios without event dispatch

## Module Implementation

Each module provides a thin implementation inheriting from base classes. The module-specific `ProcessOutboxJob` class extends `ProcessOutboxJobBase` and configures the module's database context and event handlers.

## Diagrams

### Event Propagation: In-Process vs Out-of-Process

![Event Propagation](docs/event-propagation.png)

### Event Hierarchy

![Event Hierarchy](docs/event-hierarchy.png)
