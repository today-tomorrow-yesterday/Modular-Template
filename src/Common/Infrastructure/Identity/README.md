# Identity

Provides current user context from HTTP claims.

## CurrentUserService

Implements `ICurrentUserService` - retrieves user info from `HttpContext.User`.

| Property | Source |
|----------|--------|
| `UserId` | `ClaimTypes.NameIdentifier` |
| `UserName` | `Identity.Name` |
| `IsAuthenticated` | `Identity.IsAuthenticated` |

## Usage

Inject `ICurrentUserService` where you need user context:

```csharp
public class CreateOrderHandler(ICurrentUserService currentUser)
{
    public async Task<Result<Guid>> Handle(...)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedException();
        // Use userId
    }
}
```

Used automatically by:
- `AuditableEntitiesInterceptor` (sets CreatedByUserId, ModifiedByUserId)
- `SoftDeleteInterceptor` (sets DeletedByUserId)
- `AuditTrailInterceptor` (logs who made changes)
