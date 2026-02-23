# Resilience Patterns

This folder contains resilience infrastructure for chaos engineering readiness, implementing circuit breaker, retry, and timeout patterns using [Polly](https://github.com/App-vNext/Polly).

## Overview

The resilience layer protects the application from cascading failures when external dependencies (AWS EventBridge, SQS, Redis, etc.) become slow or unavailable.

## Components

| Component | Purpose |
|-----------|---------|
| `ResilienceOptions` | Configuration for retry, circuit breaker, and timeout policies |
| `ResilientEventBridgeEventBus` | Decorator that wraps `IEventBus` with resilience patterns |

## Resilience Pipeline

The resilience pipeline is applied in the following order (outermost to innermost):

```
┌─────────────────────────────────────────────────────────────┐
│  1. Total Timeout                                           │
│     Caps entire operation including all retries             │
│  ┌───────────────────────────────────────────────────────┐  │
│  │  2. Retry                                             │  │
│  │     Retries failed attempts with exponential backoff  │  │
│  │  ┌─────────────────────────────────────────────────┐  │  │
│  │  │  3. Circuit Breaker                             │  │  │
│  │  │     Fails fast when error threshold exceeded    │  │  │
│  │  │  ┌───────────────────────────────────────────┐  │  │  │
│  │  │  │  4. Attempt Timeout                       │  │  │  │
│  │  │  │     Caps each individual attempt          │  │  │  │
│  │  │  │                                           │  │  │  │
│  │  │  │     → External Call (EventBridge, etc.)   │  │  │  │
│  │  │  │                                           │  │  │  │
│  │  │  └───────────────────────────────────────────┘  │  │  │
│  │  └─────────────────────────────────────────────────┘  │  │
│  └───────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
```

## Circuit Breaker States

```
     ┌────────────────────────────────────────────┐
     │                  CLOSED                    │
     │    (Normal operation - calls go through)   │
     │                                            │
     │  Tracks: failure count / total requests    │
     └──────────────────┬─────────────────────────┘
                        │
            Failure threshold exceeded
            (e.g., 20% failures over 10s)
                        │
                        ▼
     ┌────────────────────────────────────────────┐
     │                   OPEN                     │
     │   (Fail-fast - immediately reject calls)   │
     │                                            │
     │          Wait for BreakDuration            │
     └──────────────────┬─────────────────────────┘
                        │
              Break duration elapsed
                        │
                        ▼
     ┌────────────────────────────────────────────┐
     │               HALF-OPEN                    │
     │    (Test the waters - allow one call)      │
     │                                            │
     │   Success → CLOSED   |   Failure → OPEN    │
     └────────────────────────────────────────────┘
```

## Logging

The resilience layer logs important events:

| Event | Level | Description |
|-------|-------|-------------|
| Retry attempt | Warning | Each retry with attempt number and delay |
| Circuit opened | Error | Circuit breaker tripped due to failures |
| Circuit half-open | Information | Circuit testing if service recovered |
| Circuit closed | Information | Service recovered, normal operation resumed |
| Attempt timeout | Warning | Individual attempt exceeded timeout |
| Total timeout | Error | Entire operation exceeded total timeout |

Example log output:
```
[WRN] Retry attempt 1/3 for EventBus publish after 1000ms. Exception: Request timeout
[WRN] Retry attempt 2/3 for EventBus publish after 2000ms. Exception: Request timeout
[ERR] Circuit breaker OPENED for EventBus. Break duration: 30s. Failure ratio exceeded threshold.
[INF] Circuit breaker HALF-OPEN for EventBus. Testing with next request
[INF] Circuit breaker CLOSED for EventBus. Resuming normal operations
```


## Integration with Outbox Pattern

The resilience layer works alongside the outbox pattern:

1. **Outbox** ensures events are persisted before publishing
2. **Resilience** handles transient failures during publish
3. If resilience exhausts retries, the outbox will retry later
4. Circuit breaker prevents overwhelming a failing EventBridge

```
Outbox Job                    Resilient EventBus
──────────                    ──────────────────
Read pending messages
        │
        ▼
PublishAsync() ──────────────→ Timeout → Retry → Circuit → AWS
        │                              ↑    │
        │                              └────┘ (on failure)
        │
        ▼
Mark as processed (or retry later)
```

## Related Documentation

- [Event Bus](../EventBus/README.md) - Event publishing and handling
- [Outbox Pattern](../Outbox/README.md) - Reliable event persistence
- [Polly Documentation](https://www.pollydocs.org/) - Resilience library docs
