---
name: logging-observability
description: Use when adding logging to handlers, event processors, or infrastructure components - covers structured logging patterns, log levels, and correlation
---

# Logging & Observability

## Structured Logging with Microsoft.Extensions.Logging

This project uses `ILogger<T>` via dependency injection. All log messages use structured logging with named placeholders — never string interpolation.

### Correct

```csharp
logger.LogInformation(
    "Processing ProductCreated: PublicProductId={PublicProductId}, Name={Name}",
    integrationEvent.PublicProductId,
    integrationEvent.Name);
```

### Wrong

```csharp
// DON'T: string interpolation loses structure
logger.LogInformation($"Processing ProductCreated: {integrationEvent.PublicProductId}");

// DON'T: concatenation
logger.LogInformation("Processing ProductCreated: " + integrationEvent.PublicProductId);
```

## Log Levels

| Level | When to Use | Example |
|-------|-------------|---------|
| `LogDebug` | Detailed diagnostic info, disabled in production | Query parameters, cache hits/misses |
| `LogInformation` | Normal operations completing successfully | "Processing OrderPlaced", "Customer created" |
| `LogWarning` | Something unexpected but recoverable | "OrderCache not found for status update", remove events, retries |
| `LogError` | Operation failed, needs attention | Unhandled exceptions, external service failures |
| `LogCritical` | Application-level failure | Startup failures, data corruption |

## Patterns by Layer

### Integration Event Handlers

```csharp
// ADD/CREATE events → LogInformation
logger.LogInformation(
    "Processing ProductCreated: PublicProductId={PublicProductId}, Name={Name}",
    integrationEvent.PublicProductId,
    integrationEvent.Name);

// REMOVE/DELETE events → LogWarning (data loss, needs visibility)
logger.LogWarning(
    "Processing OrderRemoved: PublicOrderId={PublicOrderId}",
    integrationEvent.PublicOrderId);

// Cache miss during update → LogWarning
logger.LogWarning(
    "OrderCache not found for PublicOrderId={PublicOrderId}. Status update skipped.",
    integrationEvent.PublicOrderId);
```

### Command Handlers

Generally don't log — let the integration event handler or endpoint log instead. Exception: long-running operations or operations with side effects.

```csharp
// Only when there's something worth tracking
logger.LogInformation(
    "Order {PublicOrderId} status changed: {OldStatus} → {NewStatus}",
    order.PublicId, oldStatus, newStatus);
```

### Outbox/Inbox Processing

```csharp
// Successful processing
logger.LogDebug("Processed outbox message {MessageId}", message.Id);

// Failed processing (retryable)
logger.LogWarning(
    "Failed to process outbox message {MessageId}, attempt {RetryCount}: {Error}",
    message.Id, message.RetryCount, exception.Message);

// Failed processing (permanent)
logger.LogError(exception,
    "Permanently failed to process outbox message {MessageId} after {RetryCount} attempts",
    message.Id, message.RetryCount);
```

## What to Log

**Always log:**
- Integration event processing (type + identity)
- Entity state transitions (status changes, lifecycle advances)
- External service calls (request/response summary)
- Cache removal events (data loss)
- Retry attempts and failures

**Never log:**
- `[SensitiveData]` fields (addresses, phone, email, financial amounts)
- Full request/response bodies (use correlation IDs instead)
- Passwords, tokens, API keys, connection strings
- PII in any form

**Use PublicId in logs, never int Id:**
```csharp
// CORRECT
logger.LogInformation("Customer {PublicCustomerId} updated", customer.PublicId);

// WRONG — leaks internal ID
logger.LogInformation("Customer {CustomerId} updated", customer.Id);
```

## Injecting ILogger

```csharp
// Primary constructor injection (preferred)
internal sealed class OrderPlacedIntegrationEventHandler(
    ISender sender,
    ILogger<OrderPlacedIntegrationEventHandler> logger)
    : IntegrationEventHandler<OrderPlacedIntegrationEvent>
```

The `ILogger<T>` generic parameter should always match the containing class for proper log categorization.

## Checklist

- [ ] Structured placeholders (not string interpolation)
- [ ] Correct log level (Info for normal, Warning for recoverable issues, Error for failures)
- [ ] Integration event handlers log event type + PublicId on entry
- [ ] Remove/delete events logged at Warning level
- [ ] No `[SensitiveData]` fields in log messages
- [ ] PublicId used in logs, never int Id
- [ ] `ILogger<T>` generic matches containing class
