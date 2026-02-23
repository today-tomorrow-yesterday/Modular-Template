namespace Rtl.Core.Domain.Entities;

/// <summary>
/// Interface for entities that support soft deletion.
/// Entities implementing this interface will have their records marked as deleted
/// instead of being physically removed from the database.
/// </summary>
/// <remarks>
/// The soft delete fields are automatically populated by the SoftDeleteInterceptor
/// when an entity is deleted via EF Core's Remove method.
/// </remarks>
public interface ISoftDeletable
{
    bool IsDeleted { get; }

    DateTime? DeletedAtUtc { get; }

    Guid? DeletedByUserId { get; }
}
