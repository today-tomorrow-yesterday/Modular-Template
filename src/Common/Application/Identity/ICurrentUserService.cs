namespace ModularTemplate.Application.Identity;

/// <summary>
/// Service for accessing the current authenticated user's information.
/// Used by audit interceptors and authorization logic.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the current user's unique identifier.
    /// Returns null if no user is authenticated.
    /// </summary>
    Guid? UserId { get; }

    /// <summary>
    /// Gets the current user's username or email.
    /// Returns null if no user is authenticated.
    /// </summary>
    string? UserName { get; }

    /// <summary>
    /// Indicates whether a user is currently authenticated.
    /// </summary>
    bool IsAuthenticated { get; }
}
