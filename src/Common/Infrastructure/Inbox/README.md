# Inbox Pattern

## What Is It?

The **Transactional Inbox** ensures reliable integration event handling. When a module receives an event from another module (via the event bus), it's first persisted to an `inbox_messages` table. A background job then processes these events with idempotency guarantees.

This ensures **exactly-once processing** - even if the same event is delivered multiple times.

## How It Works

```
1. Integration event received from event bus
2. Event persisted to inbox_messages table
3. Background job polls inbox_messages table
4. Event dispatched to handlers (with idempotency check)
5. Message marked as processed (or scheduled for retry)
```

## What's in This Folder?

| File | Purpose |
|------|---------|
| `InboxOptions.cs` | Configuration (interval, batch size, max retries) |
| `Job/ProcessInboxJobBase.cs` | Abstract base job - modules inherit from this |
| `Job/ConfigureProcessInboxJob.cs` | Registers the inbox processing job with DI |
| `IdempotentIntegrationEventHandlerBase.cs` | Base decorator for idempotent handling |
| `IntegrationEventHandlersFactory.cs` | Discovers handlers via reflection |
| `Persistence/InboxMessage.cs` | The inbox message entity |
| `Persistence/InboxMessageConsumer.cs` | Tracks which handlers processed each message |
| `Persistence/InboxMessageConfiguration.cs` | EF Core entity configuration for InboxMessage |
| `Persistence/InboxMessageConsumerConfiguration.cs` | EF Core entity configuration for InboxMessageConsumer |

## Retry & Dead Letter

Failed messages are retried with exponential backoff:
- **Delays:** 5s → 30s → 2min → 10min → 1hr
- **Max retries:** 5 (configurable)
- **Dead letter:** After max retries, message is marked processed with error

## Module Implementation

Each module provides a thin implementation inheriting from `ProcessInboxJobBase`. See any module's `Infrastructure/Inbox/ProcessInboxJob.cs` for an example.
