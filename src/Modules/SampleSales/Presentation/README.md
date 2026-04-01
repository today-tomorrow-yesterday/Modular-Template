# Module Presentation Layer

The Presentation Layer handles all external inputs and acts as an **Anti-Corruption Layer (ACL)** between the outside world and the module's domain.

## Architecture

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

## Why ACL Matters

- External contracts (DTOs, events) evolve independently of domain models
- Domain logic stays pure and unaware of delivery mechanisms
- Same command can be triggered by HTTP, events, or other inputs

## Module Boundaries

- **CAN** depend on: `Common.*` layers, this module's Application layer, other modules' IntegrationEvents
- **CANNOT** depend on: other modules' Domain/Application/Infrastructure, this module's Infrastructure

## What Belongs Here

- **Endpoints** - translate HTTP requests into commands/queries
- **Integration Event Handlers** - consume events from other modules and map to internal commands
- **DTOs** - request/response models for this module's API
- **Validation/Documentation** - endpoint-specific attributes

## Key Patterns

- **API Flow**: `HTTP Request -> Endpoint -> Command/Query -> Handler -> Domain`
- **Event Flow**: `Integration Event -> Handler -> Domain Services -> Database`
- **Module Communication**: via Integration Events only (never direct API calls or shared DB)

## API Versioning

- Use `.MapToApiVersion(new ApiVersion(x, y))` on endpoints
- Organize versions in separate folders (V1/, V2/)
- Both versions coexist for client migration flexibility

## Feature Flags

- Gate endpoints with `.RequireFeature(FeatureName)` - returns 404 when disabled
- Define flags in the Application layer (e.g., `SampleSalesFeatures.cs`)
- Configure in `modules.samplesales.json` under `SampleSales:Features`

## References

- [Anti-Corruption Layer Pattern](https://learn.microsoft.com/en-us/azure/architecture/patterns/anti-corruption-layer)
- [Minimal APIs](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis)
- [Vertical Slice Architecture](https://www.jimmybogard.com/vertical-slice-architecture/)
