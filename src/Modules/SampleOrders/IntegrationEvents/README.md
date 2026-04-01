# SampleOrders Integration Events

Public contracts (events) that the SampleOrders module publishes for other modules to consume. This is the **only** project from SampleOrders that other modules may reference.

## Event Flow

```
SampleOrders Module (Publisher)
───────────────────────────────────────────────────────────────────
Domain Layer          Application Layer              Infrastructure
───────────────────────────────────────────────────────────────────
Order.Place()
  │
  ▼
OrderPlacedDomainEvent ──▶ OrderPlacedDomainEventHandler
                                       │
                                       ▼
                             IEventBus.PublishAsync(
                                OrderPlacedIntegrationEvent)
                                       │
                                       ▼
                                       Outbox ──▶ Message Bus
```

Consumer (SampleSales)
────────────────────────────────────────────────────────────
Bus ──▶ Inbox ──▶ ProcessInboxJob ──▶ Handler ──▶ Local Cache
```
Sales Module (Consumer)
───────────────────────────────────────────────────────────────────
Infrastructure                  Presentation Layer (ACL)
───────────────────────────────────────────────────────────────────
Message Bus
  │
  ▼
IntegrationEventConsumer
  │
  ▼
inbox_messages table
  │
  ▼
ProcessInboxJob ──────────────▶ OrderPlacedIntegrationEventHandler
                                  │
                                  │ (updates local cache)
                                  ▼
                                IOrderCacheWriter.UpsertAsync()
```

## Key Concepts

- **Source of truth**: SampleOrders owns order data; consumers maintain local caches
- **Loose coupling**: Modules communicate only via integration events
- **Eventual consistency**: The inbox/outbox pattern handles failures gracefully

## Dependency Rule

This project has **NO dependencies** on other layers or modules. It is a pure contract package containing only:
- Integration event record definitions
- Shared primitive types if needed

```
┌─────────────────────────────────────────────────────────────────┐
│                         Other Modules                           │
│   (Can ONLY reference this IntegrationEvents project)           │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│       SampleOrders.IntegrationEvents (this project)             │
│   • OrderPlacedIntegrationEvent                                 │
│   • OrderStatusChangedIntegrationEvent                          │
└─────────────────────────────────────────────────────────────────┘
                              ▲
                              │ (publishes)
┌─────────────────────────────────────────────────────────────────┐
│                  SampleOrders Module Internal                    │
│   • Domain (domain events trigger integration events)           │
│   • Application (handlers publish integration events)           │
│   • Infrastructure (sends to message bus)                       │
└─────────────────────────────────────────────────────────────────┘
```

## References

- [Integration Events](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/multi-container-microservice-net-applications/integration-event-based-microservice-communications) - Microsoft
- [Event-Driven Architecture](https://martinfowler.com/articles/201701-event-driven.html) - Martin Fowler
