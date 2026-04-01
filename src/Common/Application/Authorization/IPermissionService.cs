using ModularTemplate.Domain.Results;

namespace ModularTemplate.Application.Authorization;

/// <summary>
/// Service for retrieving user permissions.
/// </summary>
public interface IPermissionService
{
    /// <summary>
    /// Gets the permissions for a user.
    /// </summary>
    Task<Result<PermissionsResponse>> GetUserPermissionsAsync(string identityId);
}
