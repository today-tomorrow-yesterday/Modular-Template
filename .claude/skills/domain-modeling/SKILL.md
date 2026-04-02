---
name: domain-modeling
description: Use when creating domain entities, value objects, aggregate roots, or domain events - covers entity patterns, Result types, domain errors, and factory methods
---

# Domain Modeling Patterns

## Entity Classification

| Type | Inherits | Implements | When |
|------|----------|------------|------|
| Aggregate Root | `SoftDeletableEntity` | `IAggregateRoot` | Module owns this data (Customer, Order, Product, Catalog) |
| CDC Cache with Events | `Entity` | `ICacheProjection` | External data with change detection |
| Simple Cache | — | `ICacheProjection` | External data, no events (ProductCache, OrderCache) |
| Child Entity | `Entity` | — | Owned by an aggregate (CustomerContact, CustomerAddress, OrderLine, ShippingAddress) |
| Value Object | — | — | Immutable, equality by value (CustomerName, Address, Email) |

## Aggregate Root Pattern

```csharp
public sealed class {Entity} : SoftDeletableEntity, IAggregateRoot
{
    private readonly List<{Child}> _{children} = [];

    private {Entity}() { }  // EF constructor

    public Guid PublicId { get; private set; }
    // ... properties with { get; private set; }

    public IReadOnlyCollection<{Child}> {Children} => _{children}.AsReadOnly();

    // ─── Factory Methods ───
    public static Result<{Entity}> Create(/* params */)
    {
        if (/* validation failure */)
        {
            return Result.Failure<{Entity}>({Entity}Errors.SomeError);
        }

        var entity = new {Entity}
        {
            PublicId = Guid.CreateVersion7(),
            // ... set properties
        };
        entity.Raise(new {Entity}CreatedDomainEvent());
        return entity;
    }

    // ─── Behavioral Methods ───
    public Result DoSomething(/* params */)
    {
        if (/* business rule violation */)
        {
            return Result.Failure({Entity}Errors.SomeError);
        }

        // ... apply changes
        Raise(new {Entity}SomethingHappenedDomainEvent());
        return Result.Success();
    }
}
```

**Inheritance chain:** `SoftDeletableEntity` → `AuditableEntity` → `Entity` → (Id, DomainEvents, Raise)

**Rules:**
- `sealed class` — no inheritance beyond the base chain
- Aggregate roots extend `SoftDeletableEntity` (provides audit + soft delete)
- Private parameterless constructor for EF
- Factory methods return `Result<{Entity}>` with validation (never `new {Entity}()` from outside)
- `Guid.CreateVersion7()` for PublicId
- `Raise()` domain events in factory methods and behavioral methods
- Return `Result`/`Result<T>` from all public methods, not exceptions
- Always use curly braces on if/else — even single-line returns
- Collections exposed as `IReadOnlyCollection<T>` via `.AsReadOnly()`

## Value Object Pattern

```csharp
public sealed record CustomerName
{
    private CustomerName() { }

    public string FirstName { get; private init; } = string.Empty;
    public string? MiddleName { get; private init; }
    public string LastName { get; private init; } = string.Empty;
    public string? NameExtension { get; private init; }

    public static CustomerName Create(
        string firstName, string? middleName, string? lastName, string? nameExtension = null)
    {
        return new CustomerName
        {
            FirstName = firstName,
            MiddleName = middleName,
            LastName = lastName ?? string.Empty,
            NameExtension = nameExtension
        };
    }
}
```

**Rules:**
- Use `record` for automatic equality
- Private constructor + static `Create()` factory
- `private init` setters (immutable after creation)

## Result Pattern

```csharp
// Success
return Result.Success();
return Result.Success(data);

// Failure
return Result.Failure(SomeErrors.NotFound);
return Result.Failure<Guid>(SomeErrors.ValidationFailed);

// In handlers
var result = entity.DoSomething();
if (result.IsFailure) return result;
```

## Domain Errors

```csharp
public static class {Entity}Errors
{
    // Parameterized errors — use static methods
    public static Error NotFound(Guid publicId) =>
        Error.NotFound("{Entities}.NotFound", $"The {entity} with PublicId '{publicId}' was not found.");

    // Fixed errors — use static readonly fields
    public static readonly Error NameEmpty =
        Error.Validation("{Entities}.NameEmpty", "The {entity} name cannot be empty.");

    public static readonly Error InvalidStatusTransition =
        Error.Validation("{Entities}.InvalidStatusTransition", "Invalid status transition.");
}
```

Error codes: `{Entities}.{ErrorName}` (plural entity name prefix). Use static methods for errors with parameters, static readonly fields for fixed errors.

## Domain Events

```csharp
// Parameterless (handler loads entity from DB)
public sealed record {Entity}CreatedDomainEvent : DomainEvent;

// With identity (entity may be gone when handler runs)
public sealed record {Entity}RemovedDomainEvent(Guid PublicId) : DomainEvent;

// With change data (when handler needs specific context)
public sealed record {Entity}HomeCenterChangedDomainEvent(int NewHomeCenterNumber) : DomainEvent;
```

**EntityId** is set automatically by the `Raise()` method infrastructure — don't set it in the record constructor.

## Repository Interface

```csharp
// Full write access (aggregate roots)
public interface I{Entity}Repository : IRepository<{Entity}, int>
{
    Task<{Entity}?> GetByPublicIdAsync(Guid publicId, CancellationToken ct = default);
    // ... query methods
}

// Read-only (cache projections)
public interface I{Entity}Repository : IReadRepository<{Entity}, int>
{
    // ... query-only methods
}
```

## Command/Query Separation

**Commands** modify state, return `Result` or `Result<T>`:
```csharp
public sealed record {Action}Command(/* params */) : ICommand;
public sealed record {Action}Command(/* params */) : ICommand<Guid>;  // returns PublicId
```

**Queries** read state, return `Result<T>`:
```csharp
public sealed record Get{Entity}Query(Guid PublicId) : IQuery<{Response}>;
public sealed record Get{Entities}Query(int HomeCenterNumber) : IQuery<IReadOnlyCollection<{Response}>>;
```

## Validators (FluentValidation)

```csharp
internal sealed class {Command}Validator : AbstractValidator<{Command}>
{
    public {Command}Validator()
    {
        RuleFor(x => x.HomeCenterNumber)
            .GreaterThan(0)
            .WithMessage("HomeCenterNumber is required");
    }
}
```

## Checklist

- [ ] Aggregate roots: `sealed class`, `SoftDeletableEntity, IAggregateRoot`, private ctor, factory methods returning `Result<T>`
- [ ] `Guid.CreateVersion7()` for PublicId in factory methods
- [ ] Domain events raised in factory and behavioral methods
- [ ] Behavioral methods return `Result`/`Result<T>`, not exceptions
- [ ] Value objects use `record` with `Create()` factory
- [ ] Collections exposed via `IReadOnlyCollection<T>`
- [ ] Error codes follow `{Entities}.{ErrorName}` pattern
