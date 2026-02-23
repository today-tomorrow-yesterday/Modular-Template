# Module Presentation Layer

This is the **Presentation Layer** for the Sample module. It handles all external inputs to the module and acts as an **Anti-Corruption Layer (ACL)** between the outside world and the module's domain.

## The Presentation Layer as an Anti-Corruption Layer

The Presentation layer serves as the module's ACL, protecting the domain from external concerns:

```
┌─────────────────────────────────────────────────────────────┐
│                    EXTERNAL INPUTS                          │
│  ┌─────────────┐    ┌─────────────────────────────────┐     │
│  │ HTTP        │    │ Integration Events              │     │
│  │ Requests    │    │ (from other modules)            │     │
│  └──────┬──────┘    └───────────────┬─────────────────┘     │
└─────────┼───────────────────────────┼───────────────────────┘
          │                           │
          ▼                           ▼
┌─────────────────────────────────────────────────────────────┐
│              PRESENTATION LAYER (ACL)                       │
│  ┌─────────────┐    ┌─────────────────────────────────┐     │
│  │ Endpoints   │    │ Integration Event Handlers      │     │
│  │             │    │                                 │     │
│  │ Translate   │    │ Translate external events       │     │
│  │ HTTP → Cmd  │    │ into internal commands          │     │
│  └──────┬──────┘    └───────────────┬─────────────────┘     │
└─────────┼───────────────────────────┼───────────────────────┘
          │                           │
          ▼                           ▼
┌─────────────────────────────────────────────────────────────┐
│                  APPLICATION LAYER                          │
│                Commands / Queries / Handlers                │
└─────────────────────────────────────────────────────────────┘
```

**Why this matters:**
- External contracts (HTTP DTOs, integration events) can change independently of domain models
- The ACL translates external representations into internal commands
- Domain logic remains pure and unaware of delivery mechanisms
- Same command can be triggered by HTTP, events, or other inputs

## Module Boundaries

The Presentation layer of a module:

- **CAN** depend on `Common.*` layers (except Infrastructure) and this module's Domain/Application
- **CAN** depend on other modules' `IntegrationEvents` projects (to consume their published events)
- **CANNOT** depend on other modules' Domain/Application/Infrastructure
- **CANNOT** depend on this module's Infrastructure layer

## What Belongs Here

### API Endpoints
HTTP endpoints that translate requests into application commands/queries:


### Integration Event Handlers
Handlers that consume events published by other modules:


**Integration event handlers live here because:**
1. They handle **external inputs** - events from other modules are external to this module
2. They act as **translators** - mapping external event schemas to internal commands
3. They provide **consistency** - all entry points (HTTP, events) follow the same pattern
4. They enable **independent evolution** - external event contracts can change without affecting domain logic

### Other Components
- Request/Response DTOs specific to this module's API
- Endpoint-specific validation attributes
- OpenAPI/Swagger documentation attributes

## Module Isolation

Each module exposes its own set of endpoints. Modules communicate through:
- **Integration Events** (asynchronous, via message bus)

Never through direct API calls or shared database access between modules.

## API Design

Endpoints translate HTTP requests into application commands/queries:

```
HTTP Request → Endpoint → Command/Query → Handler → Domain → Response
```

## Integration Event Design

Integration event handlers translate external module events into internal commands:

```
Integration Event → Handler → Command → Handler → Domain
```

## References

- [Anti-Corruption Layer Pattern](https://learn.microsoft.com/en-us/azure/architecture/patterns/anti-corruption-layer) - Microsoft
- [Minimal APIs](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis) - Microsoft
- [Vertical Slice Architecture](https://www.jimmybogard.com/vertical-slice-architecture/) - Jimmy Bogard
