# Domain Layer

The innermost layer of Clean Architecture containing enterprise business rules, completely independent of external concerns.

## What Belongs Here

- **Entities** - Objects with identity that encapsulate business logic
- **Value Objects** - Immutable objects defined by their attributes
- **Domain Events** - Notifications when something meaningful happens
- **Aggregates** - Entity/value object clusters treated as a single unit
- **Repository Interfaces** - Abstractions for persistence (implementations in Infrastructure)

## Result Pattern

Functional error handling that avoids exceptions for expected failures.

- `Result.Success()` / `Result.Success(value)` - Return success
- `Result.Failure(error)` - Return failure with error details
- `result.Match(onSuccess, onFailure)` - Handle both cases cleanly

**Error Types:** `Error.Failure()`, `Error.Validation()`, `Error.NotFound()`, `Error.Problem()`, `Error.Conflict()`

## Entity Base Classes

- **AuditableEntity** - Tracks `CreatedByUserId`, `CreatedAtUtc`, `ModifiedByUserId`, `ModifiedAtUtc` (auto-populated by interceptor)
- **SoftDeletableEntity** - Extends auditable with soft delete support; tracks `IsDeleted`, `DeletedAtUtc`, `DeletedByUserId`; includes `Restore()` method

## Repository Interfaces

- **IReadRepository<TEntity, TId>** - Read-only operations: `GetByIdAsync`, `GetAllAsync`
- **IRepository<TEntity, TId>** - Extends read with write operations: `Add`, `Update`, `Remove`, etc.

## Other Abstractions

- **IDateTimeProvider** - Testable abstraction for `UtcNow`
- **ICacheProjection** - Marker for cache entities with `LastSyncedAtUtc` tracking
- **IAuditable** - Marker for field-level audit logging
- **ISoftDeletable** - Marker for soft deletion support

## References

- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html) - Robert C. Martin
- [Domain-Driven Design Reference](https://www.domainlanguage.com/ddd/reference/) - Eric Evans
- [.NET Architecture Guides](https://learn.microsoft.com/en-us/dotnet/architecture/) - Microsoft
