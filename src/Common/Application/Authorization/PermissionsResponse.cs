namespace ModularTemplate.Application.Authorization;

/// <summary>
/// Response containing user permissions.
/// </summary>
public sealed record PermissionsResponse(Guid UserId, HashSet<string> Permissions);
