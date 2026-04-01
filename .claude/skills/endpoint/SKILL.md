---
name: scaffold-endpoint
description: Use when adding a new Minimal API endpoint - creates endpoint class, request/response DTOs, command/query, handler, and Swagger metadata
---

# Scaffold Endpoint

Creates a complete Minimal API endpoint following the project's conventions.

## Endpoint Structure

Each endpoint is a class in `src/Modules/{Module}/Presentation/Endpoints/{Feature}/V1/`.

### Query Endpoint (GET)

**File:** `src/Modules/{Module}/Presentation/Endpoints/{Feature}/V1/{EndpointName}Endpoint.cs`

```csharp
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.{Module}.Application.{Feature}.{QueryName};
using ModularTemplate.Presentation.Endpoints;
using ModularTemplate.Presentation.Results;

namespace Modules.{Module}.Presentation.Endpoints.{Feature}.V1;

internal sealed class {EndpointName}Endpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapGet("/{routeParam:int}", Get{Entity}ByIdAsync)
            .WithName("Get{Entity}ById")           // REQUIRED for Swagger
            .WithSummary("Get a {entity} by ID")
            .WithDescription("Retrieves a {entity} by its unique identifier.")
            .MapToApiVersion(new ApiVersion(1, 0))
            .Produces<ApiEnvelope<{Response}>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> Get{Entity}ByIdAsync(
        int {routeParam},
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new {QueryName}({routeParam});

        var result = await sender.Send(query, cancellationToken);

        return result.Match(ApiResponse.Ok, ApiResponse.Problem);
    }
}
```

### Command Endpoint (POST — Create)

```csharp
internal sealed class Create{Entity}Endpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapPost("/", Create{Entity}Async)
            .WithName("Create{Entity}")            // REQUIRED for Swagger
            .WithSummary("Create a new {entity}")
            .WithDescription("Creates a new {entity}.")
            .MapToApiVersion(new ApiVersion(1, 0))
            .Produces<ApiEnvelope<Create{Entity}Response>>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> Create{Entity}Async(
        Create{Entity}Request request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new Create{Entity}Command(request.Prop1, request.Prop2);

        var result = await sender.Send(command, cancellationToken);

        return result.Match(
            publicId => ApiResponse.Created($"/{entities}/{publicId}", new Create{Entity}Response(publicId)),
            ApiResponse.Problem);
    }
}

public sealed record Create{Entity}Request(string Prop1, string? Prop2);
public sealed record Create{Entity}Response(Guid PublicId);
```

### Command Endpoint (PUT/PATCH — Update)

```csharp
internal sealed class Update{Entity}Endpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapPut("/{entityId:int}/{subResource}", Update{Entity}Async)
            .WithName("Update{Entity}")            // REQUIRED for Swagger
            .WithSummary("Update {entity}")
            .WithDescription("Updates an existing {entity}.")
            .MapToApiVersion(new ApiVersion(1, 0))
            .Produces<ApiEnvelope<object>>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> Update{Entity}Async(
        int entityId,
        Update{Entity}Request request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new Update{Entity}Command(entityId, request.Prop1);

        var result = await sender.Send(command, cancellationToken);

        return result.Match(
            () => ApiResponse.Success(),
            ApiResponse.Problem);
    }
}

public sealed record Update{Entity}Request(string Prop1);
```

### Nested Resource Endpoint (POST — Add child to aggregate)

```csharp
// POST /customers/{customerId}/contacts — adds a contact to a customer
internal sealed class Add{Child}Endpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapPost("/{parentId:int}/{children}", Add{Child}Async)
            .WithName("Add{Parent}{Child}")        // REQUIRED for Swagger
            .WithSummary("Add a {child} to a {parent}")
            .WithDescription("Adds a {child} to an existing {parent}.")
            .MapToApiVersion(new ApiVersion(1, 0))
            .Produces<ApiEnvelope<object>>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> Add{Child}Async(
        int parentId,
        Add{Child}Request request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new Add{Child}Command(parentId, request.Prop1, request.Prop2);

        var result = await sender.Send(command, cancellationToken);

        return result.Match(
            () => ApiResponse.Success(),
            ApiResponse.Problem);
    }
}

public sealed record Add{Child}Request(string Prop1, string? Prop2);
```

## Response DTOs

**File:** `src/Modules/{Module}/Application/{Feature}/{QueryName}/{Response}.cs`

```csharp
public sealed record {Response}(
    Guid PublicId,              // ALWAYS Guid — NEVER int Id
    // ... fields
    ShippingAddressResponse? ShippingAddress,  // nullable child DTOs
    DateTime CreatedAtUtc,
    Guid CreatedByUserId,
    DateTime? ModifiedAtUtc,
    Guid? ModifiedByUserId);

// Child response DTOs in same file
public sealed record ShippingAddressResponse(
    string? AddressLine1,
    string? AddressLine2,
    string? City,
    string? State,
    string? PostalCode,
    string? Country);
```

**Rules:**
- Use `Guid PublicId` for entity identity — never `int Id`
- Drop "Ref" prefix from response properties
- Nested DTOs for complex sub-objects (e.g., `ShippingAddressResponse`, `OrderLineResponse`)

## Key Patterns

**Handler method names MUST be unique per endpoint class** — use descriptive names like `Get{Entity}ByIdAsync`, `Create{Entity}Async`, `Add{Child}Async`. This ensures unique Swagger `operationId` values.

**Route parameters** are passed directly in the method signature (e.g., `int customerId`), NOT via `[AsParameters]`.

**Result.Match patterns:**
- GET queries: `result.Match(ApiResponse.Ok, ApiResponse.Problem)`
- Commands returning void: `result.Match(() => ApiResponse.Success(), ApiResponse.Problem)`
- Commands returning data: `result.Match(data => ApiResponse.Created(...), ApiResponse.Problem)`

## Swagger Rules

**Every endpoint MUST have `.WithName("UniqueOperationId")`.**

The `CustomOperationIds` in `Api/Shared/OpenApiExtensions.cs` checks `EndpointNameMetadata` first, falls back to method name. `.WithName()` sets `EndpointNameMetadata`.

**Naming convention for operation IDs:**
- `Get{Entity}ById` — single entity by ID
- `GetAll{Entities}` — collection
- `Create{Entity}` — POST new entity
- `Update{Entity}` or `Set{SubResource}` — PUT/PATCH
- `Add{Parent}{Child}` — nested resource POST
- `Delete{Entity}` — DELETE

## Checklist

- [ ] `.WithName("UniqueOperationId")` present — Swagger will break without it
- [ ] `.MapToApiVersion(new ApiVersion(1, 0))` present
- [ ] `.WithSummary()` and `.WithDescription()` present
- [ ] Handler method name is unique and descriptive (NOT `HandleAsync`)
- [ ] Method signature: `RouteGroupBuilder group` (NOT `IEndpointRouteBuilder app`)
- [ ] Route is relative to group (NOT `/api/v1/...`)
- [ ] Response uses `Guid PublicId` — never `int Id`
- [ ] `.Produces<>()` and `.ProducesProblem()` for OpenAPI schema
- [ ] Result matched with `ApiResponse.Ok`, `ApiResponse.Created`, or `ApiResponse.Success()`
- [ ] Errors matched with `ApiResponse.Problem`
