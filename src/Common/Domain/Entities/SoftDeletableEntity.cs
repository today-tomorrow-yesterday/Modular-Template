namespace Rtl.Core.Domain.Entities;

/// <summary>
/// Base class for auditable entities that support soft deletion.
/// Combines audit trail tracking with soft delete functionality.
/// </summary>
/// <remarks>
/// Entities inheriting from this class will:
/// - Have audit fields populated by AuditableEntitiesInterceptor
/// - Have soft delete fields populated by SoftDeleteInterceptor
/// - Be automatically filtered out of queries when deleted (via global query filter)
/// </remarks>
public abstract class SoftDeletableEntity : AuditableEntity, ISoftDeletable
{
    public bool IsDeleted { get; internal set; }

    public DateTime? DeletedAtUtc { get; internal set; }

    public Guid? DeletedByUserId { get; internal set; }

    /// <summary>
    /// Restores a previously soft-deleted entity.
    /// </summary>
    /// <remarks>
    /// Call this method to undo a soft delete. The entity will become
    /// visible in queries again after calling SaveChanges.
    /// </remarks>
    public virtual void Restore()
    {
        IsDeleted = false;
        DeletedAtUtc = null;
        DeletedByUserId = null;
    }
}
