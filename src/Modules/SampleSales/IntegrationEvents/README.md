# SampleSales Integration Events

Public contracts (events) that SampleSales publishes for other modules to consume. This is the **only** project from SampleSales that other modules may reference.

## Event Flow

```
Publisher (SampleSales)                    Consumer (e.g., SampleOrders)
───────────────────────────                ────────────────────────────────
Domain Event                               Message Bus
    │                                          │
    ▼                                          ▼
Domain Event Handler                       Inbox Table
    │                                          │
    ▼                                          ▼
Outbox ──▶ Message Bus ──────────────────▶ ProcessInboxJob ──▶ Handler
```

## Key Concepts

| Concept | Description |
|---------|-------------|
| **Source of Truth** | SampleSales owns product data; consumers maintain local caches |
| **Loose Coupling** | Modules communicate via events, no direct dependencies |
| **Eventual Consistency** | Inbox/Outbox pattern handles failures and retries |

## Dependency Rule

```
Other Modules ──▶ SampleSales.IntegrationEvents ◀── SampleSales Internal
                  (this project)
┌─────────────────────────────────────────────────────────────────┐
│                         Other Modules                           │
│   (Can ONLY reference this IntegrationEvents project)           │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│            Sales.IntegrationEvents (this project)               │
│   • ProductCreatedIntegrationEvent                              │
│   • ProductUpdatedIntegrationEvent                              │
└─────────────────────────────────────────────────────────────────┘
                              ▲
                              │ (publishes)
┌─────────────────────────────────────────────────────────────────┐
│                     Sales Module Internal                       │
│   • Domain (domain events trigger integration events)           │
│   • Application (handlers publish integration events)           │
│   • Infrastructure (sends to message bus)                       │
└─────────────────────────────────────────────────────────────────┘
```

This project depends only on **Common.Application** for the `IntegrationEvent` base class.

## References

- [Integration Events](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/multi-container-microservice-net-applications/integration-event-based-microservice-communications) - Microsoft
- [Event-Driven Architecture](https://martinfowler.com/articles/201701-event-driven.html) - Martin Fowler
