namespace Rtl.Core.Domain.Entities;

/// <summary>
/// Base class for entities that require audit trail tracking.
/// Inherits domain event support from Entity and implements IAuditableEntity.
/// </summary>
/// <remarks>
/// Entities inheriting from this class will automatically have their
/// audit fields populated by the AuditableEntitiesInterceptor during SaveChanges.
/// </remarks>
public abstract class AuditableEntity : Entity, IAuditableEntity
{
    public Guid CreatedByUserId { get; internal set; }

    public DateTime CreatedAtUtc { get; internal set; }

    public Guid? ModifiedByUserId { get; internal set; }

    public DateTime? ModifiedAtUtc { get; internal set; }
}
