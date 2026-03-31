---
name: error-handling
description: Use when implementing error handling - covers the full error flow from domain errors through Result types to ProblemDetails API responses
---

# Error Handling — Domain to API

This project uses a layered error handling approach: domain errors → Result types → ProblemDetails responses. No exceptions for business logic.

## Layer 1: Domain Errors

**File:** `src/Modules/{Module}/Domain/{Aggregate}/{Entity}Errors.cs`

```csharp
public static class OrderErrors
{
    public static readonly Error NotFound =
        Error.NotFound("Orders.NotFound", "Order not found.");

    public static readonly Error CustomerRequired =
        Error.Validation("Orders.CustomerRequired", "A customer is required to place an order.");

    public static readonly Error CannotModifyNonPendingOrder =
        Error.Validation("Orders.CannotModifyNonPendingOrder", "Only pending orders can be modified.");

    public static readonly Error InvalidStatusTransition =
        Error.Validation("Orders.InvalidStatusTransition", "Invalid order status transition.");
}
```

**Error types available:**
- `Error.NotFound(code, message)` — 404
- `Error.Validation(code, message)` — 400
- `Error.Conflict(code, message)` — 409
- `Error.Failure(code, message)` — 500

**Conventions:**
- Error codes: `{Entities}.{ErrorName}` (plural entity prefix)
- Use `static readonly` fields, not methods — errors don't need dynamic parameters
- Define errors next to the entity that owns the business rule

## Layer 2: Result Type

Domain entities and command handlers return `Result` or `Result<T>` — never throw exceptions for domain logic.

**In entities (behavioral methods):**
```csharp
public Result UpdateStatus(OrderStatus newStatus)
{
    if (!IsValidStatusTransition(Status, newStatus))
    {
        return Result.Failure(OrderErrors.InvalidStatusTransition);
    }

    Status = newStatus;
    Raise(new OrderStatusChangedDomainEvent(oldStatus, newStatus));
    return Result.Success();
}
```

**In command handlers:**
```csharp
public async Task<Result<Guid>> Handle(CreateCustomerCommand request, CancellationToken ct)
{
    var result = Customer.Create(request.FirstName, request.MiddleName, request.LastName, request.Email);
    if (result.IsFailure)
    {
        return Result.Failure<Guid>(result.Error);
    }

    customerRepository.Add(result.Value);
    await unitOfWork.SaveChangesAsync(ct);
    return result.Value.PublicId;
}
```

**In query handlers:**
```csharp
public async Task<Result<CustomerResponse>> Handle(GetCustomerQuery request, CancellationToken ct)
{
    var customer = await customerRepository.GetByIdAsync(request.CustomerId, ct);
    if (customer is null)
    {
        return Result.Failure<CustomerResponse>(CustomerErrors.NotFound);
    }

    return new CustomerResponse(customer.PublicId, customer.Name.FirstName, ...);
}
```

## Layer 3: FluentValidation (Request Validation)

**File:** `src/Modules/{Module}/Application/{Feature}/{Command}/{Command}Validator.cs`

```csharp
internal sealed class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
    }
}
```

Validators run automatically via the MediatR pipeline behavior. Failed validation returns `400 Bad Request` with ProblemDetails before the handler executes.

## Layer 4: API Response (ProblemDetails)

**Endpoint uses `Result.Match()` with `ApiResponse` helpers:**

```csharp
// In endpoint HandleAsync:
var result = await sender.Send(command, cancellationToken);

return result.Match(
    publicId => ApiResponse.Created($"/customers/{publicId}", new CreateCustomerResponse(publicId)),
    ApiResponse.Problem);    // Method group — converts Error → ProblemDetails
```

**`ApiResponse` static helpers:**
- `ApiResponse.Ok(data)` — 200 with `ApiEnvelope<T>` wrapper
- `ApiResponse.Created(location, data)` — 201 with Location header
- `ApiResponse.Success()` — 200 with null data
- `ApiResponse.Problem(error)` — Maps `Error` type to HTTP status code

**Error → HTTP status mapping:**
| Error Type | HTTP Status | Example |
|-----------|-------------|---------|
| `Error.NotFound` | 404 Not Found | Entity not found by PublicId |
| `Error.Validation` | 400 Bad Request | Business rule violation |
| `Error.Conflict` | 409 Conflict | Duplicate entity |
| `Error.Failure` | 500 Internal Server Error | Unexpected failure |

**`ApiEnvelope<T>` response shape:**
```json
{
  "data": { ... },
  "errors": null,
  "isSuccess": true
}
```

```json
{
  "data": null,
  "errors": [{ "code": "Orders.NotFound", "message": "Order not found." }],
  "isSuccess": false
}
```

## Layer 5: Global Exception Handler

**File:** `src/Common/Presentation/GlobalExceptionHandler.cs`

Catches unhandled exceptions (infrastructure failures, null refs, etc.) and converts to 500 ProblemDetails. This is the safety net — domain logic should never reach here.

## Rules

1. **Domain logic → `Result` type** — never throw exceptions
2. **Infrastructure failures → exceptions** — caught by global handler
3. **Request validation → FluentValidation** — runs before handler
4. **Domain validation → Error types** — runs inside handler/entity
5. **API response → `ApiResponse.Problem()`** — consistent ProblemDetails format
6. **Never catch and swallow** — let errors propagate to the appropriate layer

## Checklist

- [ ] Domain errors defined as `static readonly Error` fields
- [ ] Error codes follow `{Entities}.{ErrorName}` pattern
- [ ] Behavioral methods return `Result`/`Result<T>`, not exceptions
- [ ] Command handlers check `result.IsFailure` before proceeding
- [ ] Endpoints use `result.Match(success, ApiResponse.Problem)`
- [ ] FluentValidation validator exists for each command with required fields
- [ ] No `try/catch` in domain or application layers
