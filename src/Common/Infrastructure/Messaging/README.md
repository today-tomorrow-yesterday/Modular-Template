# Messaging

Shared utilities for Outbox/Inbox message processing.

## Retry Policy

Exponential backoff for failed message processing.

| Retry | Delay |
|-------|-------|
| 1st | 5 seconds |
| 2nd | 30 seconds |
| 3rd | 2 minutes |
| 4th | 10 minutes |
| 5th+ | 1 hour |

## Usage

```csharp
var nextRetry = RetryPolicy.CalculateNextRetry(retryCount, DateTime.UtcNow);
```

Used by `ProcessOutboxJobBase` and `ProcessInboxJobBase` when message processing fails.
