# API Response Envelope Design

## Problem

API responses use inconsistent shapes: success returns raw data, failure returns ProblemDetails, and 204 returns nothing. Callers must handle three different response structures depending on outcome and endpoint.

## Solution

Wrap every response in a uniform envelope with three fields: `isSuccess`, `data`, and `problemDetails`. Success populates `data`; failure populates `problemDetails`. The other is always null.

## Response Shape

```typescript
{
  isSuccess: boolean,
  data: T | null,
  problemDetails: {
    type: string,          // RFC 9457 — URI reference identifying the problem type
    title: string,         // Short human-readable summary
    status: number,        // HTTP status code
    instance: string,      // Request path that generated the error
    requestId: string,     // HttpContext.TraceIdentifier
    traceId: string,       // OpenTelemetry Activity.Current?.Id
    errors: [              // Always a list — single errors have one item
      { code: string, description: string }
    ]
  } | null
}
```

## Examples

### Success with data (200, 201)

```json
{
  "isSuccess": true,
  "data": { "id": "ab743b1f-...", "name": "Primary", "grossProfit": 53000.00 },
  "problemDetails": null
}
```

### Success without data (200 — formerly 204)

```json
{
  "isSuccess": true,
  "data": null,
  "problemDetails": null
}
```

### Single error (404)

```json
{
  "isSuccess": false,
  "data": null,
  "problemDetails": {
    "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
    "title": "Not Found",
    "status": 404,
    "instance": "/api/v1/sales/c5331d22-bb8e-2c4f-bc06-589c0aad842c",
    "requestId": "0HN8Q3K1L2C5A:00000001",
    "traceId": "00-abc123def456-789xyz-01",
    "errors": [
      { "code": "Sale.NotFound", "description": "Sale with the specified ID was not found." }
    ]
  }
}
```

### Validation errors (400)

```json
{
  "isSuccess": false,
  "data": null,
  "problemDetails": {
    "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
    "title": "Bad Request",
    "status": 400,
    "instance": "/api/v1/sales",
    "requestId": "0HN8Q3K1L2C5A:00000002",
    "traceId": "00-def789abc012-345uvw-01",
    "errors": [
      { "code": "Name", "description": "Name is required." },
      { "code": "HomeCenterNumber", "description": "Must be positive." }
    ]
  }
}
```

### Unhandled exception (500)

```json
{
  "isSuccess": false,
  "data": null,
  "problemDetails": {
    "type": "https://tools.ietf.org/html/rfc7231#section-6.6.1",
    "title": "Server Failure",
    "status": 500,
    "instance": "/api/v1/sales/c5331d22-bb8e-2c4f-bc06-589c0aad842c/packages",
    "requestId": "0HN8Q3K1L2C5A:00000003",
    "traceId": "00-xyz456def789-012abc-01",
    "errors": [
      { "code": "Server.InternalError", "description": "An unexpected error occurred." }
    ]
  }
}
```

## Rules

1. Three fields always present: `isSuccess`, `data`, `problemDetails`.
2. Mutually exclusive: `data` is null on failure, `problemDetails` is null on success.
3. HTTP status codes preserved on the wire (200, 201, 400, 404, 409, 500).
4. Former 204 No Content endpoints become 200 with `data: null`.
5. `errors` is always a list inside `problemDetails` — single errors have one item.
6. ProblemDetails follows RFC 9457: `type`, `title`, `status`, `instance` plus extensions `requestId`, `traceId`, `errors`.
7. Unhandled exceptions caught by global middleware, wrapped in same envelope.
8. No environment-specific behavior — services decide error detail level.

## Implementation Approach

Replace result helpers at the endpoint level (Approach A).

### New types in Common/Presentation

- `ApiEnvelope<T>` — the three-field record.
- `ApiEnvelopeProblemDetails` — ProblemDetails with `errors` list, `requestId`, `traceId`.
- `ApiEnvelopeError` — the `{ code, description }` record.
- `ApiResponse` — static helper class: `Ok<T>()`, `Created<T>()`, `Success()`, `Problem()`.

### New middleware

- `ExceptionEnvelopeMiddleware` — catches unhandled exceptions, wraps in envelope with 500.

### Changes to endpoints (32 total)

- `Results.Ok(data)` becomes `ApiResponse.Ok(data)`.
- `Results.Created(location, data)` becomes `ApiResponse.Created(location, data)`.
- `Results.NoContent()` becomes `ApiResponse.Success()`.
- `ApiResults.Problem` becomes `ApiResponse.Problem`.
- `.Produces<T>()` becomes `.Produces<ApiEnvelope<T>>()`.

### What stays the same

- `Result<T>` in domain and application layers.
- `Error`, `ErrorType`, `ValidationError` types.
- Endpoint routing, versioning, and request types.
