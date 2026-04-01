# Modular Monolith Template

.NET 10 modular monolith with DDD, CQRS, and clean architecture. Two sample modules (SampleOrders, SampleSales) demonstrate every pattern.

## Architecture

```
src/
├── Common/                          # Shared infrastructure (Domain, Application, Infrastructure, Presentation)
├── Modules/
│   ├── SampleOrders/                # Orders + Customers module
│   │   ├── Domain/                  # Entities, value objects, events, repository interfaces
│   │   ├── Application/             # Commands, queries, handlers, validators
│   │   ├── Infrastructure/          # EF Core, repositories, module registration
│   │   ├── Presentation/            # Endpoints, integration event handlers
│   │   ├── IntegrationEvents/       # Cross-module event contracts
│   │   └── test/                    # Domain.Tests, Application.Tests
│   └── SampleSales/                 # Products + Catalogs module (same structure)
└── Api/
    ├── Host/                        # Combined API (all modules)
    ├── Host.SampleOrders/           # Per-module API host
    ├── Host.SampleSales/            # Per-module API host
    └── Shared/                      # OpenAPI, versioning, middleware
```

**Layer dependencies:** Domain → Application → Infrastructure/Presentation → API

**Cross-module communication:** Integration events ONLY. Modules never reference each other's Domain/Application layers. Cache projections store local read copies of other modules' data.

## Critical Conventions

### Code Style
- **Always use curly braces** on if/else — even single-line returns. No exceptions.
- `sealed class` for all entities and handlers
- Primary constructors for handlers and repositories

### Identity
- `int Id` — internal, never exposed outside the module
- `Guid PublicId` — external-facing, created with `Guid.CreateVersion7()`, unique indexed
- API responses use `Guid PublicId` — NEVER `int Id`
- Commands return `Guid` (PublicId), not `int`
- Integration events use `Guid Public{EntityName}Id` (e.g., `PublicCustomerId`, `PublicProductId`)
- Cache entities use `Guid RefPublicId` (short form)

### Entity Patterns
- Aggregate roots: `sealed class {Entity} : SoftDeletableEntity, IAggregateRoot`
- Child entities: `sealed class {Child} : Entity`
- Factory methods return `Result<{Entity}>` with validation — never public constructors
- Behavioral methods return `Result` / `Result<T>` — never throw exceptions
- Domain events raised via `Raise(new {Event}())` in factory and behavioral methods

### Error Patterns
- Error codes: `{Entities}.{ErrorName}` (plural prefix, e.g., `Orders.NotFound`)
- Static fields for fixed errors: `public static readonly Error NameEmpty = Error.Validation(...)`
- Static methods for parameterized errors: `public static Error NotFound(int id) => Error.NotFound(...)`

### Naming
- Columns: `snake_case` (e.g., `public_id`, `ref_public_id`)
- Indexes: `ix_{table}_{column}`
- Cache tables: `{entity}_cache` in `Schemas.Cache`
- Endpoints: unique handler method names (e.g., `GetOrderByIdAsync`, not `HandleAsync`)

### Endpoints
- Method signature: `MapEndpoint(RouteGroupBuilder group)` — NOT `IEndpointRouteBuilder`
- Routes are relative to the group (e.g., `"/{orderId:int}"`, NOT `"/api/v1/orders/{orderId}"`)
- **Every endpoint MUST have `.WithName("UniqueOperationId")`** for Swagger
- **Every endpoint MUST have `.MapToApiVersion(new ApiVersion(1, 0))`**
- Result matching: `result.Match(ApiResponse.Ok, ApiResponse.Problem)` for queries
- Result matching: `result.Match(() => ApiResponse.Success(), ApiResponse.Problem)` for void commands
- Result matching: `result.Match(id => ApiResponse.Created(...), ApiResponse.Problem)` for create commands

### EF Core / Persistence
- JSONB details use `VersionedJsonConverter<T>` — NEVER `OwnsOne` / `ToJson`
- Details classes implement `IVersionedDetails` with `SchemaVersion` + `[JsonExtensionData] ExtensionData`
- HiLo sequences configured automatically in `ModuleDbContext` for all `Entity`-derived types — don't configure per-entity
- Cache entities (`ICacheProjection` without `Entity` base) use `.UseIdentityAlwaysColumn()`
- `DomainEvents` does NOT need `.Ignore()` — EF skips it automatically
- `[SensitiveData]` attribute auto-encrypts with AES-256-GCM via EF value converter
- **ENCRYPTION_KEY env var required** for migrations: `export ENCRYPTION_KEY="MTIzNDU2Nzg5MDEyMzQ1Njc4OTAxMjM0NTY3ODkwMTI="`

### Testing
- Every aggregate: domain tests (create, update, validation, events) + application tests (handler, validator)
- Domain tests: direct entity calls, `Result` assertions, `ClearDomainEvents()` before behavioral tests
- Application tests: NSubstitute mocks for repository + UnitOfWork, verify `Received()`/`DidNotReceive()`
- Validator tests: FluentValidation `TestValidate()` + `ShouldHaveValidationErrorFor()`

## Skills Reference

Use these skills by name when performing the corresponding task:

| Task | Skill |
|------|-------|
| Add a new module | `new-module` |
| Add an endpoint | `endpoint` |
| Add a domain entity | `domain-modeling` |
| Add an integration event | `integration-event` |
| Add a cross-module cache | `cross-module-cache` |
| Write EF configuration | `ef-core-patterns` |
| Generate/apply migrations | `migration-workflow` |
| Add error handling | `error-handling` |
| Add logging | `logging-observability` |
| Add integration tests | `testing-integration` |
| Add event flow tests | `testing-event-flow` |
| Review code | `code-review-checklist` |
| Verify before commit | `verification` |
| Check conventions | `conventions` |
| Understand module structure | `module-architecture` |

## MCP Tools (Roslyn Navigator)

This template includes a `.mcp.json` that registers the `CWM.RoslynNavigator` MCP server for semantic code analysis. Install once:

```bash
dotnet tool install -g CWM.RoslynNavigator
```

**Prefer MCP tools over file reads when possible.** They return token-optimized results (30-150 tokens vs 500+ for reading a file).

| Tool | Use When |
|------|----------|
| `find_symbol` | Locating a type, method, or property definition |
| `find_references` | Finding all usages of a symbol across the solution |
| `find_implementations` | "What implements `IRepository<T>`?" or `IEndpoint`? |
| `find_callers` | Tracing who calls a handler or domain method |
| `get_type_hierarchy` | Understanding the Entity inheritance chain |
| `get_public_api` | Seeing a type's public surface without reading the file |
| `get_symbol_detail` | Full signature, parameters, return type |
| `get_project_graph` | Module dependency map — verify no cross-module violations |
| `detect_antipatterns` | Catch `DateTime.Now`, missing `CancellationToken`, sync-over-async |
| `detect_circular_dependencies` | Enforce module boundaries at the compiler level |
| `find_dead_code` | Identify unused types and methods |
| `get_diagnostics` | Compiler warnings/errors without running `dotnet build` |

## DbContexts

| Module | DbContext | Schema |
|--------|-----------|--------|
| SampleOrders | `OrdersDbContext` | `orders` |
| SampleSales | `SampleDbContext` | `sample` |

## Don't

- Don't expose `int Id` in APIs, events, or across module boundaries
- Don't use `OwnsOne` / `ToJson` for JSONB — use `VersionedJsonConverter<T>`
- Don't throw exceptions for business rule violations — use `Result.Failure()`
- Don't reference another module's Domain/Application layer — use integration events
- Don't skip `.WithName()` on endpoints
- Don't run destructive git commands without user confirmation
- Don't omit curly braces on if/else statements
