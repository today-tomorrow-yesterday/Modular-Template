using Rtl.Core.Application.Authorization;
using Rtl.Core.Domain.Results;

namespace Rtl.Core.Infrastructure.Authorization;

/// <summary>
/// Default permission service that returns empty permissions.
/// Replace this with your own implementation when you add user management.
/// </summary>
internal sealed class DefaultPermissionService : IPermissionService
{
    public Task<Result<PermissionsResponse>> GetUserPermissionsAsync(string identityId)
    {
        // Default implementation returns empty permissions
        // Replace with actual permission lookup from your user/role system
        return Task.FromResult(Result.Success(new PermissionsResponse(Guid.Empty, [])));
    }
}
