# Presentation Layer

The Presentation Layer handles the interface between external requests and the application, translating them into commands/queries and formatting responses.

## What Belongs Here

- **API Endpoints** - REST controllers, Minimal API handlers
- **Request/Response Models** - API-specific DTOs
- **Input Validation** - Request validation and sanitization
- **Error Handling** - Global exception handling, problem details
- **Authentication/Authorization** - Security concerns at the boundary

## Dependency Rule

Presentation depends on Application and Domain layers. It invokes use cases through Application and never bypasses it to access Domain directly (except for result types like `Result<T>` and `Error`).

## Endpoint Hierarchy

- **IEndpoint** - Single HTTP operation handler (one per GET, POST, PUT, DELETE)
- **IResourceEndpoints** - Groups endpoints for a resource (like a controller)
- **IModuleEndpoints** - Module-level registration with versioning support

**Folder structure:** `Endpoints/{Resource}/V1/{Operation}Endpoint.cs`

## Minimal APIs

Each endpoint implements `IEndpoint.MapEndpoint(RouteGroupBuilder group)` using standard mapping methods: `MapGet`, `MapPost`, `MapPut`, `MapDelete`, `MapPatch`.

## API Versioning

Uses URL-based versioning via `Asp.Versioning.Http` with pattern `/api/v{version}/resource`.

- Call `.MapToApiVersion(new ApiVersion(1, 0))` to assign an endpoint to a version
- Multiple versions of the same endpoint can coexist

## Feature Flags

Use `.RequireFeature(FeatureName)` to protect endpoints.

- **Enabled:** Request proceeds normally
- **Disabled:** Returns 404 Not Found
- **Missing service (dev):** Fails open
- **Missing service (prod):** Throws exception

## MediatR / CQRS Integration

Endpoints inject `ISender` to dispatch commands/queries: map request DTO to command, call `sender.Send()`, convert `Result<T>` to HTTP response via `ApiResults`.

## References

- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html) - Robert C. Martin
- [ASP.NET Core Web API Best Practices](https://learn.microsoft.com/en-us/aspnet/core/web-api/) - Microsoft
- [REST API Guidelines](https://github.com/microsoft/api-guidelines) - Microsoft
